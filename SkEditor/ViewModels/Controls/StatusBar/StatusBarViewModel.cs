using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.StatusBar;

namespace SkEditor.ViewModels.Controls;

public partial class StatusBarViewModel : ObservableObject
{
    private readonly INotificationService _notifs;
    private readonly IWindowDialogService _windowDialogs;

    public StatusBarViewModel(
        IWindowDialogService windowDialogs,
        INotificationService notifs,
        IEnumerable<IStatusBarProvider> providers)
    {
        _windowDialogs = windowDialogs;
        _notifs = notifs;

        foreach (IStatusBarProvider provider in providers)
        {
            provider.AttachStatusChanged += OnAttachStatusChanged;
            provider.Initialize();
        }
    }

    private void OnAttachStatusChanged(object? sender, EventArgs e)
    {
        if (sender is not IStatusBarProvider provider)
        {
            return;
        }

        if (provider.CanAttach)
        {
            foreach (StatusBarItem item in provider.GetItems())
            {
                if (item.Alignment == StatusBarItem.ItemAlignment.Left)
                {
                    LeftItems.Add(item);
                }
                else
                {
                    RightItems.Add(item);
                }
            }
        }
        else
        {
            foreach (StatusBarItem item in provider.GetItems())
            {
                if (item.Alignment == StatusBarItem.ItemAlignment.Left)
                {
                    LeftItems.Remove(item);
                }
                else
                {
                    RightItems.Remove(item);
                }
            }
        }
    }

    public ObservableCollection<StatusBarItem> LeftItems { get; } = [];
    public ObservableCollection<StatusBarItem> RightItems { get; } = [];

    public List<Notification> Notifications => _notifs.GetHistory().Reverse().ToList();


    [RelayCommand]
    public async Task OpenGoToLineDialog()
    {
        await _windowDialogs.ShowGoToLineDialogAsync();
    }

    [RelayCommand]
    public void ClearAllNotifications()
    {
        _notifs.ClearHistory();
        OnPropertyChanged(nameof(Notifications));
    }

    [RelayCommand]
    public void ClearNotification(Notification notification)
    {
        _notifs.RemoveFromHistory(notification);
        OnPropertyChanged(nameof(Notifications));
    }
}