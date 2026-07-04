using System.Collections.Generic;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using R3;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings;
using SkEditor.ViewModels.Controls;

namespace SkEditor.Utilities.StatusBar;

public class EditorErrorStatusBarProvider : IStatusBarProvider
{
    private readonly StatusBarItem _item;
    private DisposableBag _disposables;

    public EditorErrorStatusBarProvider(BottomDrawerViewModel drawer, ExperimentsConfig experiments)
    {
        _item = new StatusBarItem()
            .WithIcon(Symbol.ErrorCircle)
            .WithIcon(Symbol.SpinnerIos)
            .WithText(TranslatableString.FromRawString("2"))
            .WithColor(Color.Parse("#A0A5AF"))
            .WithAlignment(StatusBarItem.ItemAlignment.Left);

        RelayCommand command = new(drawer.ToggleDrawer);

        _item.WithCommand(command);

        CanAttach = experiments
            .ObservePropertyChanged(x => x.BottomDrawerEnabled)
            .ToReadOnlyReactiveProperty(experiments.BottomDrawerEnabled)
            .AddTo(ref _disposables);
    }

    public ReadOnlyReactiveProperty<bool> CanAttach { get; }

    public IEnumerable<StatusBarItem> GetItems()
    {
        yield return _item;
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}