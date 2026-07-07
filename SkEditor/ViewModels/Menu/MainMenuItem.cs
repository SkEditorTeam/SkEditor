using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using R3;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Menu;

public class MainMenuItem : ObservableObject
{
    public BindableReactiveProperty<bool> IsVisible { get; set; } = new(true);

    public BindableReactiveProperty<bool> IsEnabled { get; set; } = new(true);

    public TranslatableString? Header { get; private set; }
    public Symbol? IconSymbol { get; private set; }
    public IRelayCommand? Command { get; private set; }
    public IEnumerable<MainMenuItem>? SubItems { get; private set; }
    public string? InputGestureText { get; private set; }
    public bool RegisterHotKey { get; private set; } = true;
    public bool IsSeparator { get; private set; }

    public static MainMenuItem Separator => new() { IsSeparator = true };

    public static MainMenuItem HeaderOnly(TranslatableString header)
    {
        return new MainMenuItem { Header = header };
    }

    public static MainMenuItem IconOnly(Symbol iconSymbol)
    {
        return new MainMenuItem { IconSymbol = iconSymbol };
    }

    public static MainMenuItem HeaderWithIcon(TranslatableString header, Symbol iconSymbol)
    {
        return new MainMenuItem
        {
            Header = header,
            IconSymbol = iconSymbol
        };
    }

    public MainMenuItem WithItem(MainMenuItem subItem)
    {
        SubItems = SubItems is not null ? [..SubItems, subItem] : [subItem];
        return this;
    }

    public MainMenuItem WithItems(IEnumerable<MainMenuItem> subItems)
    {
        SubItems = SubItems is not null ? [..SubItems, ..subItems] : subItems;
        return this;
    }

    public MainMenuItem WithIcon(Symbol iconSymbol)
    {
        IconSymbol = iconSymbol;
        return this;
    }

    public MainMenuItem WithCommand(IRelayCommand? command)
    {
        Command = command;
        return this;
    }

    public MainMenuItem WithCommand(Action execute)
    {
        Command = new RelayCommand(execute);
        return this;
    }

    public MainMenuItem WithCommand(Func<Task> execute)
    {
        Command = new AsyncRelayCommand(execute);
        return this;
    }

    public MainMenuItem WithInputGesture(string text, bool registerHotkey = true)
    {
        InputGestureText = text;
        RegisterHotKey = registerHotkey;
        return this;
    }

    public MainMenuItem WithEnabled(BindableReactiveProperty<bool> isEnabled)
    {
        IsEnabled = isEnabled;
        return this;
    }

    public MainMenuItem WithVisibility(BindableReactiveProperty<bool> isVisible)
    {
        IsVisible = isVisible;
        return this;
    }

    public void AddTo(MainMenuItem parent)
    {
        parent.WithItem(this);
    }
}