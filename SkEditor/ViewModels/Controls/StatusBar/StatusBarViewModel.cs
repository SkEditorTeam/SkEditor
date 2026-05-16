using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Notifications;

namespace SkEditor.ViewModels.Controls;

public partial class StatusBarViewModel(IWindowDialogService windowDialogs, INotificationService notifs)
    : ObservableObject
{
    public List<Notification> Notifications => notifs.GetHistory().Reverse().ToList();

    [RelayCommand]
    public async Task OpenGoToLineDialog()
    {
        await windowDialogs.ShowGoToLineDialogAsync();
    }

    [RelayCommand]
    public void ClearAllNotifications()
    {
        notifs.ClearHistory();
        OnPropertyChanged(nameof(Notifications));
    }

    [RelayCommand]
    public void ClearNotification(Notification notification)
    {
        notifs.RemoveFromHistory(notification);
        OnPropertyChanged(nameof(Notifications));
    }
}