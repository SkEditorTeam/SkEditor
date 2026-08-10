using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Analyzer;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Settings;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Controls.StatusBar.Panes;
using SkEditor.ViewModels.Tabs;
using SkEditor.ViewModels.Tabs.Capabilities;

#pragma warning disable CS8846 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

namespace SkEditor.Utilities.StatusBar;

public class EditorErrorStatusBarProvider : IStatusBarProvider, IRecipient<AnalyzerLoadedMessage>,
    IRecipient<AnalyzerStatusChangedMessage>,
    IRecipient<AnalyzerResultUpdatedMessage>,
    IRecipient<AnalyzerFailedToLoadMessage>
{
    private readonly StatusBarItem _item;
    private readonly ErrorListPaneViewModel _pane;

    private readonly Dictionary<IFileTab, AnalyzerResult> _results = new();
    private readonly ITabService _tabs;
    private readonly INotificationService _notifs;
    private DisposableBag _disposables;

    public EditorErrorStatusBarProvider(BottomDrawerViewModel drawer, ExperimentsConfig experiments, ITabService tabs,
        IMessenger messenger, INotificationService notifs)
    {
        _pane = drawer.GetPane<ErrorListPaneViewModel>() ??
                throw new InvalidOperationException("ErrorListPaneViewModel not found");

        _pane.IsAnalyzerLoaded = IsAnalyzerLoaded.ToBindableReactiveProperty();

        _item = new StatusBarItem()
            .WithIcon(Symbol.SpinnerIos)
            .WithIconClass("spin")
            .WithColor(Color.Parse("#A0A5AF"))
            .WithAlignment(StatusBarItem.ItemAlignment.Left);

        RelayCommand command = new(() => drawer.TogglePane("ErrorList"));

        _item.WithCommand(command);

        CanAttach = experiments
            .ObservePropertyChanged(x => x.AnalyzerEnabled)
            .ToReadOnlyReactiveProperty()
            .AddTo(ref _disposables);

        IsAnalyzerLoaded
            .Subscribe(_ => UpdateState())
            .AddTo(ref _disposables);

        IsAnalyzing
            .Subscribe(_ => UpdateState())
            .AddTo(ref _disposables);

        _tabs = tabs;
        _notifs = notifs;

        Observable.FromEvent<TabViewModelBase?>(
                h => tabs.SelectedTabChanged += h,
                h => tabs.SelectedTabChanged -= h)
            .Subscribe(_ => UpdateState())
            .AddTo(ref _disposables);

        messenger.Register<AnalyzerLoadedMessage>(this);
        messenger.Register<AnalyzerStatusChangedMessage>(this);
        messenger.Register<AnalyzerResultUpdatedMessage>(this);
        messenger.Register<AnalyzerFailedToLoadMessage>(this);
    }

    public ReactiveProperty<bool> IsAnalyzerLoaded { get; } = new(false);
    public ReactiveProperty<bool> IsAnalyzing { get; } = new(false);

    public void Receive(AnalyzerLoadedMessage message)
    {
        IsAnalyzerLoaded.Value = true;
    }

    public void Receive(AnalyzerResultUpdatedMessage message)
    {
        _results[message.Tab] = message.Result;
        UpdateState();
    }

    public void Receive(AnalyzerStatusChangedMessage message)
    {
        IsAnalyzing.Value = message.IsAnalyzing;
    }

    public void Receive(AnalyzerFailedToLoadMessage message)
    {
        _item.IsVisible = false;

        Notification notification = Notification
            .Create("Analyzer failed to load".ToTranslatable())
            .WithMessage(
                "The required components may be missing or incompatible. Check the settings for more information."
                    .ToTranslatable())
            .WithDuration(TimeSpan.FromSeconds(10))
            .WithType(NotificationType.Error);

        _notifs.Show(notification);
    }
    
    public ReadOnlyReactiveProperty<bool> CanAttach { get; }

    public IEnumerable<StatusBarItem> GetItems()
    {
        yield return _item;
    }

    private void UpdateState()
    {
        _pane.Errors.Clear();
        if (_tabs.SelectedTab is IFileTab fileTab && _results.TryGetValue(fileTab, out AnalyzerResult? result))
        {
            result.Errors.Sort((a, b) =>
            {
                int lineComparison = a.Line.CompareTo(b.Line);
                if (lineComparison != 0)
                {
                    return lineComparison;
                }

                int typeComparison = a.Type.CompareTo(b.Type);
                return typeComparison != 0
                    ? typeComparison
                    : string.Compare(a.Message, b.Message, StringComparison.Ordinal);
            });
            result.Errors.ForEach(_pane.Errors.Add);
        }

        bool anyErrors = _pane.Errors.Any(e => e.Type == AnalyzerErrorType.Error);
        bool anyWarnings = _pane.Errors.Any(e => e.Type == AnalyzerErrorType.Warning);

        Symbol icon = IsAnalyzerLoaded.Value switch
        {
            false or true when IsAnalyzing.Value => Symbol.SpinnerIos,
            false or true when !_pane.IsFileAnalyzable.Value => Symbol.CircleHint,
            true when anyErrors => Symbol.ErrorCircle,
            true when anyWarnings => Symbol.Warning,
            true => Symbol.CheckmarkCircle
        };

        Color color = IsAnalyzerLoaded.Value switch
        {
            false or true when !_pane.IsFileAnalyzable.Value => Color.Parse("#A0A5AF"),
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

    public void Dispose()
    {
        _disposables.Dispose();
    }
}