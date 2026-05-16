using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Windowing;

namespace SkEditor.ViewModels.Controls;

public partial class StatusBarViewModel(IWindowDialogService windowDialogs, INotificationService notifs)
    : ObservableObject
{
    [RelayCommand]
    public async Task OpenGoToLineDialog()
    {
        await windowDialogs.ShowGoToLineDialogAsync();
    }
}