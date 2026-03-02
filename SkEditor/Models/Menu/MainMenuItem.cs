using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Menu;

public partial class MainMenuItem : ObservableObject
{
    [ObservableProperty]
    private bool _isVisible = true;

    protected MainMenuItem()
    {
    }

    public TranslationKey? HeaderKey { get; init; }
    public Symbol? IconSymbol { get; init; }
    public IRelayCommand? Command { get; init; }
    public IEnumerable<MainMenuItem>? SubItems { get; init; }
    public string? InputGestureText { get; init; }
    public bool IsSeparator { get; init; }

    public static MainMenuItem Separator => new() { IsSeparator = true };

    private MainMenuItem CloneWith(
        TranslationKey? headerKey = null,
        Symbol? iconSymbol = null,
        IRelayCommand? command = null,
        IEnumerable<MainMenuItem>? subItems = null,
        string? inputGestureText = null,
        bool? isVisible = null)
    {
        return new MainMenuItem
        {
            HeaderKey = headerKey ?? HeaderKey,
            IconSymbol = iconSymbol ?? IconSymbol,
            Command = command ?? Command,
            SubItems = subItems ?? SubItems,
            InputGestureText = inputGestureText ?? InputGestureText,
            IsSeparator = IsSeparator,
            IsVisible = isVisible ?? IsVisible
        };
    }

    public static MainMenuItem HeaderOnly(TranslationKey headerKey)
    {
        return new MainMenuItem { HeaderKey = headerKey };
    }

    public static MainMenuItem IconOnly(Symbol iconSymbol)
    {
        return new MainMenuItem { IconSymbol = iconSymbol };
    }

    public static MainMenuItem HeaderWithIcon(TranslationKey headerKey, Symbol iconSymbol)
    {
        return new MainMenuItem
        {
            HeaderKey = headerKey,
            IconSymbol = iconSymbol
        };
    }

    public MainMenuItem WithItem(MainMenuItem subItem)
    {
        List<MainMenuItem> list = SubItems is not null ? [..SubItems] : [];
        list.Add(subItem);
        return CloneWith(subItems: list);
    }

    public MainMenuItem WithItems(IEnumerable<MainMenuItem> subItems)
    {
        List<MainMenuItem> list = SubItems is not null ? [..SubItems] : [];
        list.AddRange(subItems);
        return CloneWith(subItems: list);
    }

    public MainMenuItem WithIcon(Symbol iconSymbol)
    {
        return CloneWith(iconSymbol: iconSymbol);
    }

    public MainMenuItem WithCommand(IRelayCommand command)
    {
        return CloneWith(command: command);
    }

    public MainMenuItem WithCommand(Action execute)
    {
        return CloneWith(command: new RelayCommand(execute));
    }

    public MainMenuItem WithCommand(Func<Task> execute)
    {
        return CloneWith(command: new AsyncRelayCommand(execute));
    }

    public MainMenuItem WithInputGesture(string text)
    {
        return CloneWith(inputGestureText: text);
    }

    public MainMenuItem WithVisibility(bool isVisible)
    {
        return CloneWith(isVisible: isVisible);
    }
}