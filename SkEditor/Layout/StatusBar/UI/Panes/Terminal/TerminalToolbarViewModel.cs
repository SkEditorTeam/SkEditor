using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Layout.StatusBar.UI.Abstractions;

namespace SkEditor.Layout.StatusBar.UI.Panes.Terminal;

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