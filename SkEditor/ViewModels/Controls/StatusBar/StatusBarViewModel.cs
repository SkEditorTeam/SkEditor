using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.StatusBar;

namespace SkEditor.ViewModels.Controls;

public partial class StatusBarViewModel : ObservableObject, IDisposable
{
    private readonly INotificationService _notifs;
    private readonly IWindowDialogService _windowDialogs;
    private DisposableBag _disposables;

    public StatusBarViewModel(
        IWindowDialogService windowDialogs,
        INotificationService notifs,
        IEnumerable<IStatusBarProvider> providers)
    {
        _windowDialogs = windowDialogs;
        _notifs = notifs;

        foreach (IStatusBarProvider provider in providers)
        {
            provider.CanAttach
                .Subscribe(canAttach => OnAttachStatusChanged(provider, canAttach))
                .AddTo(ref _disposables);
        }
    }

    private void OnAttachStatusChanged(IStatusBarProvider provider, bool canAttach)
    {
        foreach (StatusBarItem item in provider.GetItems())
        {
            ObservableCollection<StatusBarItem> targetCollection = item.Alignment == StatusBarItem.ItemAlignment.Left
                ? LeftItems
                : RightItems;

            if (canAttach)
            {
                if (!targetCollection.Contains(item))
                {
                    targetCollection.Add(item);
                }
            }
            else
            {
                targetCollection.Remove(item);
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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _disposables.Dispose();
    }
}