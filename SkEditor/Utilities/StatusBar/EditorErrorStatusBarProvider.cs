using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using R3;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Analyzer;
using SkEditor.Utilities.Settings;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Controls.StatusBar.Panes;
using SkEditor.ViewModels.Tabs;
using SkEditor.ViewModels.Tabs.Capabilities;

#pragma warning disable CS8846 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

namespace SkEditor.Utilities.StatusBar;

public class EditorErrorStatusBarProvider : IStatusBarProvider
{
    private readonly StatusBarItem _item;
    private readonly ErrorListPaneViewModel _pane;
    private readonly ITabService _tabs;
    private DisposableBag _disposables;

    private readonly Dictionary<IFileTab, AnalyzerResult> _results = new();

    public ReactiveProperty<bool> IsAnalyzerLoaded { get; } = new(false);
    public ReactiveProperty<bool> IsAnalyzing { get; } = new(false);

    public EditorErrorStatusBarProvider(BottomDrawerViewModel drawer, ExperimentsConfig experiments, ITabService tabs)
    {
        _pane = drawer.GetPane<ErrorListPaneViewModel>() ??
                throw new InvalidOperationException("ErrorListPaneViewModel not found");

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

        IsAnalyzerLoaded
            .Subscribe(_ => UpdateState())
            .AddTo(ref _disposables);

        IsAnalyzing
            .Subscribe(_ => UpdateState())
            .AddTo(ref _disposables);

        _tabs = tabs;

        Observable.FromEvent<TabViewModelBase?>(
                h => tabs.SelectedTabChanged += h,
                h => tabs.SelectedTabChanged -= h)
            .Subscribe(_ => UpdateState())
            .AddTo(ref _disposables);
    }

    private void UpdateState()
    {
        _pane.Errors.Clear();
        if (_tabs.SelectedTab is IFileTab fileTab && _results.TryGetValue(fileTab, out AnalyzerResult? result))
        {
            result.Errors.ForEach(_pane.Errors.Add);
        }
        
        bool anyErrors = _pane.Errors.Any(e => e.Type == AnalyzerErrorType.Error);
        bool anyWarnings = _pane.Errors.Any(e => e.Type == AnalyzerErrorType.Warning);

        Symbol icon = IsAnalyzerLoaded.Value switch
        {
            false or true when IsAnalyzing.Value => Symbol.SpinnerIos,
            true when anyErrors => Symbol.ErrorCircle,
            true when anyWarnings => Symbol.Warning,
            true => Symbol.CheckmarkCircle
        };

        Color color = IsAnalyzerLoaded.Value switch
        {
            false => Color.Parse("#A0A5AF"),
            true when anyErrors => Color.Parse("#FF2D52"),
            true when anyWarnings => Color.Parse("#FFAE45"),
            true => Color.Parse("#8BF07B"),
        };

        string? iconClass = IsAnalyzerLoaded.Value switch
        {
            false or true when IsAnalyzing.Value => "spin",
            true => null
        };

        _item.WithIcon(icon);
        _item.WithIconClass(iconClass);
        _item.WithColor(color);
    }

    public void UpdateResult(IFileTab tab, AnalyzerResult result)
    {
        _results[tab] = result;
        UpdateState();
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