using System;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Controls;

public partial class StatusBarItem : ObservableObject
{
    public enum ItemAlignment
    {
        Left,
        Right
    }

    [ObservableProperty]
    public partial TranslatableString? Text { get; set; }

    [ObservableProperty]
    public partial Symbol? IconSymbol { get; set; }

    [ObservableProperty]
    public partial string? IconClass { get; set; }

    [ObservableProperty]
    public partial IRelayCommand? Command { get; set; }

    [ObservableProperty]
    public partial bool IsVisible { get; set; } = true;

    [ObservableProperty]
    public partial Color? ForegroundColor { get; set; }

    public ItemAlignment Alignment { get; private set; } = ItemAlignment.Right;

    public static StatusBarItem TextOnly(TranslatableString text)
    {
        return new StatusBarItem { Text = text };
    }

    public static StatusBarItem IconOnly(Symbol icon)
    {
        return new StatusBarItem { IconSymbol = icon };
    }

    public StatusBarItem WithText(TranslatableString text)
    {
        Text = text;
        return this;
    }

    public StatusBarItem WithIcon(Symbol icon)
    {
        IconSymbol = icon;
        return this;
    }

    public StatusBarItem WithIconClass(string iconClass)
    {
        IconClass = iconClass;
        return this;
    }

    public StatusBarItem WithCommand(IRelayCommand command)
    {
        Command = command;
        return this;
    }

    public StatusBarItem WithCommand(Action execute)
    {
        Command = new RelayCommand(execute);
        return this;
    }

    public StatusBarItem WithCommand(Func<Task> execute)
    {
        Command = new AsyncRelayCommand(execute);
        return this;
    }

    public StatusBarItem WithAlignment(ItemAlignment alignment)
    {
        Alignment = alignment;
        return this;
    }

    public StatusBarItem WithColor(Color color)
    {
        ForegroundColor = color;
        return this;
    }
}