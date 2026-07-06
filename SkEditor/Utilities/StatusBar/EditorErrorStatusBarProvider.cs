using System.Collections.Generic;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using R3;
using SkEditor.Models.Analyzer;
using SkEditor.Utilities.Settings;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Controls.StatusBar.Panes;

namespace SkEditor.Utilities.StatusBar;

public class EditorErrorStatusBarProvider : IStatusBarProvider
{
    private readonly StatusBarItem _item;
    private readonly BottomDrawerViewModel _drawer;
    private DisposableBag _disposables;

    public ReactiveProperty<bool> IsAnalyzerLoaded { get; } = new(false);
    public ReactiveProperty<bool> IsAnalyzing { get; } = new(false);

    public EditorErrorStatusBarProvider(BottomDrawerViewModel drawer, ExperimentsConfig experiments)
    {
        _drawer = drawer;
        
        _item = new StatusBarItem()
            .WithIcon(Symbol.SpinnerIos)
            .WithIconClass("spin")
            .WithColor(Color.Parse("#A0A5AF"))
            .WithAlignment(StatusBarItem.ItemAlignment.Left);

        RelayCommand command = new(drawer.ToggleDrawer);

        _item.WithCommand(command);

        CanAttach = experiments
            .ObservePropertyChanged(x => x.BottomDrawerEnabled)
            .ToReadOnlyReactiveProperty(experiments.BottomDrawerEnabled)
            .AddTo(ref _disposables);

        IsAnalyzerLoaded.AddTo(ref _disposables);
        IsAnalyzerLoaded
            .Subscribe(_ => UpdateState());

        IsAnalyzing.AddTo(ref _disposables);
        IsAnalyzing
            .Subscribe(_ => UpdateState());
    }

    private void UpdateState()
    {
        Symbol icon = IsAnalyzerLoaded.Value switch
        {
            false or true when IsAnalyzing.Value => Symbol.SpinnerIos,
            true => Symbol.CheckmarkCircle
        };

        string? iconClass = IsAnalyzerLoaded.Value switch
        {
            false or true when IsAnalyzing.Value => "spin",
            true => null
        };

        Color color = IsAnalyzerLoaded.Value switch
        {
            true when IsAnalyzing.Value => Color.Parse("#8BF07B"),
            true => Color.Parse("#8BF07B"),
            false => Color.Parse("#A0A5AF")
        };

        _item.WithIcon(icon);
        _item.WithIconClass(iconClass);
        _item.WithColor(color);
    }

    public void UpdateResult(AnalyzerResult result)
    {
        ErrorListPaneViewModel? pane = _drawer.GetPane<ErrorListPaneViewModel>();
        if (pane is null)
        {
            return;
        }

        pane.Errors.Clear();
        result.Errors.ForEach(pane.Errors.Add);
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