using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SkEditor.ViewModels.Controls.StatusBar.Panes.Terminal;

public partial class LogsToolbarViewModel(LogsPaneViewModel logsPane) : ObservableObject, IDrawerToolbarViewModel
{
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