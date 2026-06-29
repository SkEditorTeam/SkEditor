using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.ViewModels.Controls.StatusBar.Panes.Terminal;

public class TerminalToolbarViewModel(TerminalPaneViewModel terminalPane) : ObservableObject, IDrawerToolbarViewModel;