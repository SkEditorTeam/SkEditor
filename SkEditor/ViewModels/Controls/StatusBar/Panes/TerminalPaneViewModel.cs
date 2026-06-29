using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Controls.Panes;

public class TerminalPaneViewModel : ObservableObject, IDrawerPaneViewModel
{
    public TranslatableString Name => TranslatableString.FromRawString("Terminal");
}