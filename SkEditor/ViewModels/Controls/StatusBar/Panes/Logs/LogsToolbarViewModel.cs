using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using R3;
using SkEditor.Models.Logging;

namespace SkEditor.ViewModels.Controls.StatusBar.Panes.Terminal;

public partial class LogsToolbarViewModel(LogsPaneViewModel logsPane) : ObservableObject, IDrawerToolbarViewModel
{
    public IReadOnlyList<LogSource?> AvailableSources { get; } = [LogSource.SkEditor, LogSource.Analyzer];
    public BindableReactiveProperty<LogSource?> SelectedSource { get; } = new(LogSource.SkEditor);
    
    [RelayCommand]
    public void ClearLogs()
    {
        logsPane.ClearLogs();
    }

    [RelayCommand]
    public void OpenLogsFolder()
    {
        logsPane.OpenLogsFolder();
    }

    [RelayCommand]
    public async Task CopyLogs()
    {
        await logsPane.CopyLogs();
    }
}