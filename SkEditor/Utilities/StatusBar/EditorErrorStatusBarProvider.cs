using System;
using System.Collections.Generic;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings;
using SkEditor.ViewModels.Controls;

namespace SkEditor.Utilities.StatusBar;

public class EditorErrorStatusBarProvider : IStatusBarProvider
{
    private readonly StatusBarItem _item;
    private readonly ExperimentsConfig _experiments;

    public EditorErrorStatusBarProvider(BottomDrawerViewModel drawer, ExperimentsConfig experiments)
    {
        _experiments = experiments;

        _item = new StatusBarItem()
            .WithIcon(Symbol.ErrorCircle)
            .WithText(TranslatableString.FromRawString("2"))
            .WithColor(Color.Parse("#FFFF3559"))
            .WithAlignment(StatusBarItem.ItemAlignment.Left);

        RelayCommand command = new(drawer.ToggleDrawer);

        _item.WithCommand(command);
    }

    public bool CanAttach => _experiments.BottomDrawerEnabled;

    public void Initialize()
    {
        AttachStatusChanged?.Invoke(this, EventArgs.Empty);

        _experiments.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ExperimentsConfig.BottomDrawerEnabled))
            {
                AttachStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        };
    }

    public event EventHandler? AttachStatusChanged;

    public IEnumerable<StatusBarItem> GetItems()
    {
        yield return _item;
    }
}