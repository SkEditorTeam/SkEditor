using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ObservableCollections;
using R3;
using SkEditor.Core.Localization;
using SkEditor.Core.Logging;
using SkEditor.Core.System;
using SkEditor.Layout.StatusBar.UI.Abstractions;
using AppSettings = SkEditor.Configuration.Configs.AppSettings;
using TranslatableString = SkEditor.Core.Localization.TranslatableString;

namespace SkEditor.Layout.StatusBar.UI.Panes.Logs;

public class LogsPaneViewModel : ObservableObject, IDrawerPaneViewModel
{
    private readonly ObservableList<LogEntry> _allLogs = [];

    private readonly IClipboardService _clipboard;
    private readonly ILinkService _links;
    private readonly ILogService _logs;

    private DisposableBag _disposables;
    public NotifyCollectionChangedSynchronizedViewList<LogEntry> FilteredLogs { get; }
    public string Id => "Logs";
    public BindableReactiveProperty<bool> IsVisible { get; }
    public TranslatableString Name => TranslationKey.BottomDrawerLogsTitle;
    public IDrawerToolbarViewModel? Toolbar { get; }

    public LogsPaneViewModel(AppSettings appSettings, ILogService logs, IClipboardService clipboard, ILinkService links)
    {
        _clipboard = clipboard;
        _links = links;
        _logs = logs;

        IsVisible = appSettings
            .ObservePropertyChanged(x => x.IsDevModeEnabled)
            .ToBindableReactiveProperty();

        LogsToolbarViewModel toolbar = new(this);
        Toolbar = toolbar;

        ISynchronizedView<LogEntry, LogEntry> logsView = _allLogs
            .CreateView(x => x)
            .AddTo(ref _disposables);

        FilteredLogs = logsView
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .AddTo(ref _disposables);

        logs.OnLogReceived
            .ObserveOnCurrentSynchronizationContext()
            .Subscribe(entry =>
            {
                _allLogs.Add(entry);

                if (_allLogs.Count > 500)
                {
                    _allLogs.RemoveAt(0);
                }
            })
            .AddTo(ref _disposables);

        toolbar.SelectedSource
            .Subscribe(source =>
            {
                if (source is null)
                {
                    logsView.ResetFilter();
                }
                else
                {
                    logsView.AttachFilter(entry => entry.Source == source.Value);
                }
            })
            .AddTo(ref _disposables);
    }

    public void ClearLogs()
    {
        _allLogs.Clear();
    }

    public void OpenLogsFolder()
    {
        _links.OpenFolder(_logs.LogsFolderPath);
    }

    public async Task CopyLogs()
    {
        StringBuilder sb = new();
        foreach (LogEntry log in FilteredLogs)
        {
            sb.AppendLine(log.ToString());
        }

        string logs = sb.ToString();

        if (!string.IsNullOrWhiteSpace(logs))
        {
            await _clipboard.CopyTextAsync(logs);
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}