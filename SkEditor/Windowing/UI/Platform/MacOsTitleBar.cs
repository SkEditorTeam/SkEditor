using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace SkEditor.AttachedProperties;

public static partial class MacOsTitleBar
{
    public static readonly AttachedProperty<bool> IsThickProperty =
        AvaloniaProperty.RegisterAttached<Window, bool>(
            "IsThick", typeof(MacOsTitleBar));

    private static readonly ConditionalWeakTable<Window, ThickTitleBarState> States = new();

    static MacOsTitleBar() => IsThickProperty.Changed.AddClassHandler<Window>(OnIsThickChanged);

    public static bool GetIsThick(Window window)
    {
        return window.GetValue(IsThickProperty);
    }

    public static void SetIsThick(Window window, bool value)
    {
        window.SetValue(IsThickProperty, value);
    }

    private static void OnIsThickChanged(Window window, AvaloniaPropertyChangedEventArgs args)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        if (args.NewValue is true)
        {
            ThickTitleBarState state = new(window);
            States.AddOrUpdate(window, state);
            state.Attach();
        }
        else
        {
            if (!States.TryGetValue(window, out ThickTitleBarState? state))
            {
                return;
            }

            state.Detach();
            States.Remove(window);
        }
    }

    [LibraryImport("/usr/lib/libobjc.A.dylib", StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr objc_getClass(string name);

    [LibraryImport("/usr/lib/libobjc.A.dylib", StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr sel_registerName(string name);

    [LibraryImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static partial IntPtr objc_msgSend_ret(IntPtr receiver, IntPtr selector);

    [LibraryImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static partial void objc_msgSend_arg(IntPtr receiver, IntPtr selector, IntPtr arg);

    [LibraryImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static partial void objc_msgSend_bool(IntPtr receiver, IntPtr selector,
        [MarshalAs(UnmanagedType.I1)] bool arg);

    [LibraryImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static partial void objc_msgSend_long(IntPtr receiver, IntPtr selector, long arg);

    private sealed class ThickTitleBarState(Window window)
    {
        private bool _attached;
        private IntPtr _nsToolbar;
        private IntPtr _nsWindow;
        private IntPtr _selSetTitlebarAppearsTransparent;
        private IntPtr _selSetTitleVisibility;
        private IntPtr _selSetToolbar;

        public void Attach()
        {
            if (_attached)
            {
                return;
            }

            window.Closed += (_, _) => Detach();

            if (window.IsLoaded)
            {
                Setup();
            }
            else
            {
                window.Loaded += OnLoaded;
            }
        }

        public void Detach()
        {
            if (!_attached)
            {
                return;
            }

            window.Loaded -= OnLoaded;
            window.PropertyChanged -= OnWindowPropertyChanged;

            if (_nsWindow != IntPtr.Zero && _selSetToolbar != IntPtr.Zero)
            {
                objc_msgSend_arg(_nsWindow, _selSetToolbar, IntPtr.Zero);
            }

            _attached = false;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            window.Loaded -= OnLoaded;
            Setup();
        }

        private void Setup()
        {
            try
            {
                _nsWindow = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
                if (_nsWindow == IntPtr.Zero)
                {
                    Dispatcher.UIThread.Post(Setup, DispatcherPriority.Loaded);
                    return;
                }

                _selSetToolbar = sel_registerName("setToolbar:");
                _selSetTitlebarAppearsTransparent = sel_registerName("setTitlebarAppearsTransparent:");
                _selSetTitleVisibility = sel_registerName("setTitleVisibility:");

                _nsToolbar = objc_msgSend_ret(
                    objc_getClass("NSToolbar"), sel_registerName("new"));

                if (_nsToolbar == IntPtr.Zero)
                {
                    return;
                }

                objc_msgSend_bool(_nsToolbar, sel_registerName("setShowsBaselineSeparator:"), false);
                objc_msgSend_arg(_nsWindow, _selSetToolbar, _nsToolbar);
                objc_msgSend_long(_nsWindow, _selSetTitleVisibility, 1);

                objc_msgSend_long(_nsWindow, sel_registerName("setToolbarStyle:"), 3);
                objc_msgSend_bool(_nsWindow, _selSetTitlebarAppearsTransparent, true);

                window.PropertyChanged += OnWindowPropertyChanged;
                _attached = true;
            }
            catch
            {
                // ignored
            }
        }

        private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property != Window.WindowStateProperty)
            {
                return;
            }

            WindowState newState = e.GetNewValue<WindowState>();

            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    objc_msgSend_arg(_nsWindow, _selSetToolbar,
                        newState == WindowState.FullScreen ? IntPtr.Zero : _nsToolbar);

                    objc_msgSend_bool(_nsWindow, _selSetTitlebarAppearsTransparent, true);
                    objc_msgSend_long(_nsWindow, _selSetTitleVisibility, 1);
                }
                catch
                {
                    // ignored
                }
            }, DispatcherPriority.Background);
        }
    }
}