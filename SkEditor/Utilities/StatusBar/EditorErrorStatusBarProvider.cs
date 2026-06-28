using System.Collections.Generic;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;
using SkEditor.ViewModels.Controls;

namespace SkEditor.Utilities.StatusBar;

public class EditorErrorStatusBarProvider : IStatusBarProvider
{
    private readonly StatusBarItem _item;

    public enum EditorErrorStatus
    {
        AllGood,
        Error,
        Warning
    }

    private EditorErrorStatus _status = EditorErrorStatus.AllGood;

    public EditorErrorStatusBarProvider(BottomDrawerViewModel drawer)
    {
        _item = new StatusBarItem()
            .WithIcon(Symbol.CheckmarkCircle)
            .WithText(TranslatableString.FromRawString("0"))
            .WithColor(Color.Parse("#FF9BF99B"))
            .WithAlignment(StatusBarItem.ItemAlignment.Left);

        RelayCommand command = new(drawer.ToggleDrawer);

        _item.WithCommand(command);
    }

    public IEnumerable<StatusBarItem> GetItems()
    {
        yield return _item;
    }
}