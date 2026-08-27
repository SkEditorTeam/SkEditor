using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using R3;
using SkEditor.Services.Localization;
using SkEditor.ViewModels.Menu;

namespace SkEditor.AttachedProperties;

public static class NativeMenuBridge
{
    public static readonly AttachedProperty<IEnumerable<MainMenuItem>?> ItemsSourceProperty =
        AvaloniaProperty.RegisterAttached<Window, IEnumerable<MainMenuItem>?>(
            "ItemsSource", typeof(NativeMenuBridge));

    private static readonly AttachedProperty<CompositeDisposable?> MenuDisposablesProperty =
        AvaloniaProperty.RegisterAttached<Window, CompositeDisposable?>(
            "MenuDisposables", typeof(NativeMenuBridge));

    static NativeMenuBridge() => ItemsSourceProperty.Changed.AddClassHandler<Window>(OnItemsSourceChanged);

    public static IEnumerable<MainMenuItem>? GetItemsSource(Window window)
    {
        return window.GetValue(ItemsSourceProperty);
    }

    public static void SetItemsSource(Window window, IEnumerable<MainMenuItem>? value)
    {
        window.SetValue(ItemsSourceProperty, value);
    }

    private static void OnItemsSourceChanged(Window window, AvaloniaPropertyChangedEventArgs args)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        window.GetValue(MenuDisposablesProperty)?.Dispose();
        CompositeDisposable disposables = new();
        window.SetValue(MenuDisposablesProperty, disposables);

        Observable.FromEvent<EventHandler<string>, string>(
                h => (_, e) => h(e),
                h => TranslationService.Current.LanguageChanged += h,
                h => TranslationService.Current.LanguageChanged -= h)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => RebuildMenu(window))
            .AddTo(disposables);

        if (args.NewValue is IEnumerable<MainMenuItem> newItems)
        {
            if (newItems is INotifyCollectionChanged notifyCollection)
            {
                Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                        h => (_, e) => h(e),
                        h => notifyCollection.CollectionChanged += h,
                        h => notifyCollection.CollectionChanged -= h)
                    .Subscribe(_ => RebuildMenu(window))
                    .AddTo(disposables);
            }

            RebuildMenu(window);
        }
        else
        {
            NativeMenu.SetMenu(window, null);
        }
    }

    private static void RebuildMenu(Window window)
    {
        IEnumerable<MainMenuItem>? items = GetItemsSource(window);
        if (items is null)
        {
            NativeMenu.SetMenu(window, null);
            return;
        }

        NativeMenu? existingMenu = NativeMenu.GetMenu(window);
        if (existingMenu is null)
        {
            existingMenu = new NativeMenu();
            NativeMenu.SetMenu(window, existingMenu);
        }

        existingMenu.Items.Clear();
        PopulateNativeMenu(existingMenu, items);
    }

    private static void PopulateNativeMenu(NativeMenu targetMenu, IEnumerable<MainMenuItem> items)
    {
        foreach (MainMenuItem item in items)
        {
            if (item.IsSeparator)
            {
                targetMenu.Items.Add(new NativeMenuItemSeparator());
                continue;
            }

            NativeMenuItem nativeItem = new()
            {
                Header = item.Header?.Value,
                Command = item.Command
            };

            if (!string.IsNullOrEmpty(item.InputGestureText))
            {
                string gesture = item.InputGestureText.Replace("Ctrl", "Cmd");
                nativeItem.Gesture = KeyGesture.Parse(gesture);
            }

            if (item.SubItems is not null && item.SubItems.Any())
            {
                NativeMenu subMenu = [];
                PopulateNativeMenu(subMenu, item.SubItems);
                nativeItem.Menu = subMenu;
            }

            targetMenu.Items.Add(nativeItem);
        }
    }
}