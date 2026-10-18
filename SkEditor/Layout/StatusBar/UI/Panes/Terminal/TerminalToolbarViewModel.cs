using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SkEditor.ViewModels.Controls.StatusBar.Panes.Terminal;

public partial class TerminalToolbarViewModel(TerminalPaneViewModel terminalPane)
    : ObservableObject, IDrawerToolbarViewModel
{
    [RelayCommand]
    public async Task Reset()
    {
        await terminalPane.RestartTerminal();
    }

    [RelayCommand]
    public async Task Clear()
    {
        await terminalPane.Clear();
    }
}