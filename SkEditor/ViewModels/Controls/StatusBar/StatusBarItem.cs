using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Controls;

public partial class StatusBarItem : ObservableObject
{
    [ObservableProperty]
    public partial TranslatableString? Text { get; set; }

    [ObservableProperty]
    public partial Symbol? IconSymbol { get; set; }

    [ObservableProperty]
    public partial IRelayCommand? Command { get; set; }

    [ObservableProperty]
    public partial bool IsVisible { get; set; } = true;

    public enum ItemAlignment
    {
        Left,
        Right
    }

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
}