using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using R3;
using SkEditor.Core.Localization;
using SkEditor.Layout.Menu.Models;
using AppSettings = SkEditor.Configuration.Configs.AppSettings;

namespace SkEditor.Windowing.UI.Platform;

public static class NativeMenuBridge
{
    public static readonly AttachedProperty<IEnumerable<MainMenuItem>?> ItemsSourceProperty =
        AvaloniaProperty.RegisterAttached<Window, IEnumerable<MainMenuItem>?>(
            "ItemsSource", typeof(NativeMenuBridge));

    private static readonly ConditionalWeakTable<Window, WindowMenuState> States = new();

    private static bool IsEnabled => !App.Services.GetRequiredService<AppSettings>().UseIntegratedMenuOnMacOs;

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
        if (!OperatingSystem.IsMacOS() || !IsEnabled)
        {
            return;
        }

        if (args.NewValue is IEnumerable<MainMenuItem> newItems)
        {
            WindowMenuState state = States.GetOrCreateValue(window);
            state.RootDisposables.Clear();

            Observable.FromEvent<EventHandler<string>, string>(
                    h => (_, e) => h(e),
                    h => TranslationService.Current.LanguageChanged += h,
                    h => TranslationService.Current.LanguageChanged -= h)
                .ObserveOnUIThreadDispatcher()
                .Subscribe(_ => RebuildMenu(window))
                .AddTo(state.RootDisposables);

            if (newItems is INotifyCollectionChanged notifyCollection)
            {
                Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                        h => (_, e) => h(e),
                        h => notifyCollection.CollectionChanged += h,
                        h => notifyCollection.CollectionChanged -= h)
                    .Subscribe(_ => RebuildMenu(window))
                    .AddTo(state.RootDisposables);
            }

            Observable.FromEventHandler(
                    h => window.Closed += h,
                    h => window.Closed -= h)
                .Take(1)
                .Subscribe(_ =>
                {
                    state.Dispose();
                    States.Remove(window);
                })
                .AddTo(state.RootDisposables);
        }
        else
        {
            if (States.TryGetValue(window, out WindowMenuState? state))
            {
                state.Dispose();
                States.Remove(window);
            }
        }

        RebuildMenu(window);
    }

    private static void RebuildMenu(Window window)
    {
        IEnumerable<MainMenuItem>? items = GetItemsSource(window);
        if (items is null || !States.TryGetValue(window, out WindowMenuState? state))
        {
            NativeMenu.SetMenu(window, null);
            return;
        }

        state.ItemDisposables.Clear();

        NativeMenu? existingMenu = NativeMenu.GetMenu(window);
        if (existingMenu is null)
        {
            existingMenu = [];
            NativeMenu.SetMenu(window, existingMenu);
        }

        existingMenu.Items.Clear();
        PopulateNativeMenu(existingMenu, items, state.ItemDisposables);
    }

    private static void PopulateNativeMenu(NativeMenu targetMenu, IEnumerable<MainMenuItem> items,
        CompositeDisposable itemDisposables)
    {
        foreach (MainMenuItem item in items)
        {
            if (item.IsSeparator)
            {
                targetMenu.Items.Add(new NativeMenuItemSeparator());
                continue;
            }

            if (item.Header is null)
            {
                continue;
            }

            NativeMenuItem nativeItem = new()
            {
                Header = item.Header.Value,
                Command = item.Command
            };

            item.IsEnabled.Subscribe(val => nativeItem.IsEnabled = val).AddTo(itemDisposables);
            item.IsVisible.Subscribe(val => nativeItem.IsVisible = val).AddTo(itemDisposables);

            if (!string.IsNullOrEmpty(item.InputGestureText))
            {
                string gesture = item.InputGestureText.Replace("Ctrl", "Cmd");
                nativeItem.Gesture = KeyGesture.Parse(gesture);
            }

            if (item.SubItems is not null && item.SubItems.Any())
            {
                NativeMenu subMenu = [];
                PopulateNativeMenu(subMenu, item.SubItems, itemDisposables);
                nativeItem.Menu = subMenu;
            }

            targetMenu.Items.Add(nativeItem);
        }
    }

    private sealed class WindowMenuState : IDisposable
    {
        public readonly CompositeDisposable RootDisposables = new();
        public readonly CompositeDisposable ItemDisposables = new();

        public void Dispose()
        {
            ItemDisposables.Dispose();
            RootDisposables.Dispose();
        }
    }
}