using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Controls.Panes;

public partial class ErrorListPaneViewModel(INotificationService notifs) : ObservableObject, IDrawerPaneViewModel
{
    public TranslatableString Name => TranslatableString.FromRawString("Errors");

    [RelayCommand]
    public void ErrorClick()
    {
        notifs.Debug("you clicked the error");
    }
}