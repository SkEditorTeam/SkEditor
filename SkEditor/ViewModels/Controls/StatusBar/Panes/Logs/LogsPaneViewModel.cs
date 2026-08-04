using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Models.Logging;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Controls.StatusBar.Panes.Terminal;

namespace SkEditor.ViewModels.Controls.StatusBar.Panes;

public class LogsPaneViewModel : ObservableObject, IDrawerPaneViewModel
{
    public string Id => "Logs";
    public TranslatableString Name => "Logs".ToTranslatable();
    public BindableReactiveProperty<bool> IsVisible { get; }
    public IDrawerToolbarViewModel? Toolbar { get; }

    private DisposableBag _disposables;

    public ObservableCollection<LogEntry> Logs { get; } = [];

    private readonly IClipboardService _clipboard;
    private readonly ILinkService _links;
    private readonly ILogService _logs;

    public LogsPaneViewModel(AppSettings appSettings, ILogService logs, IClipboardService clipboard, ILinkService links)
    {
        _clipboard = clipboard;
        _links = links;
        _logs = logs;

        IsVisible = appSettings
            .ObservePropertyChanged(x => x.IsDevModeEnabled)
            .ToBindableReactiveProperty();

        Toolbar = new LogsToolbarViewModel(this);

        logs.OnLogReceived
            .ObserveOnCurrentSynchronizationContext()
            .Subscribe(entry =>
            {
                Logs.Add(entry);

                if (Logs.Count > 500)
                {
                    Logs.RemoveAt(0);
                }
            })
            .AddTo(ref _disposables);
    }

    public void ClearLogs()
    {
        Logs.Clear();
    }

    public void OpenLogsFolder()
    {
        _links.OpenFolder(_logs.LogsFolderPath);
    }

    public async Task CopyLogs()
    {
        StringBuilder sb = new();
        foreach (LogEntry log in Logs)
        {
            sb.AppendLine(log.ToString());
        }

        await _clipboard.CopyTextAsync(sb.ToString());
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}