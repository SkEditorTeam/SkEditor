using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;

// ReSharper disable InconsistentNaming

namespace SkEditor.AttachedProperties;

public static partial class DwmMaximizeFix
{
    public static readonly AttachedProperty<bool> EnableFixProperty =
        AvaloniaProperty.RegisterAttached<Window, bool>(
            "EnableFix",
            typeof(DwmMaximizeFix));

    public static bool GetEnableFix(Window element)
    {
        return element.GetValue(EnableFixProperty);
    }

    public static void SetEnableFix(Window element, bool value)
    {
        element.SetValue(EnableFixProperty, value);
    }

    static DwmMaximizeFix() =>
        EnableFixProperty.Changed.AddClassHandler<Window>((window, e) =>
        {
            if (e.NewValue is true)
            {
                Attach(window);
            }
        });

    private const int DWMWA_NCRENDERING_POLICY = 2;
    private const int DWMNCRP_DISABLED = 1;
    private const int WM_WINDOWPOSCHANGING = 0x0046;
    private const int WM_NCCALCSIZE = 0x0083;
    private const int WM_GETMINMAXINFO = 0x0024;
    private const int WM_DWMNCRENDERINGCHANGED = 0x031F;

    [LibraryImport("dwmapi.dll")]
    private static partial void DwmSetWindowAttribute(
        IntPtr hwnd,
        int dwAttribute,
        ref int pvAttribute,
        int cbAttribute);

    private static void Attach(Window window)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        bool isInitialShow = true;

        window.PropertyChanged += (_, e) =>
        {
            if (e.Property == Window.WindowStateProperty && e.NewValue is WindowState state &&
                state != WindowState.Maximized)
            {
                isInitialShow = false;
            }
        };

        Win32Properties.AddWndProcHookCallback(window, (hWnd, msg, wParam, _, ref _) =>
        {
            if (!isInitialShow || window.WindowState != WindowState.Maximized)
            {
                return IntPtr.Zero;
            }

            switch (msg)
            {
                case WM_WINDOWPOSCHANGING or WM_NCCALCSIZE or WM_GETMINMAXINFO:
                {
                    int policy = DWMNCRP_DISABLED;
                    DwmSetWindowAttribute(hWnd, DWMWA_NCRENDERING_POLICY, ref policy, sizeof(int));
                    break;
                }
                case WM_DWMNCRENDERINGCHANGED:
                {
                    bool ncRenderingEnabled = wParam != IntPtr.Zero;
                    if (ncRenderingEnabled)
                    {
                        int policy = DWMNCRP_DISABLED;
                        DwmSetWindowAttribute(hWnd, DWMWA_NCRENDERING_POLICY, ref policy, sizeof(int));
                    }

                    break;
                }
            }

            return IntPtr.Zero;
        });
    }
}