using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Models.UI;

namespace SkEditor.ViewModels.Controls;

public class SidebarViewModel(INotificationService notifications) : ObservableObject
{
    public List<SidebarItem> Items { get; } =
    [
        new("Explorer", Symbol.Folder, new RelayCommand(() => notifications.Debug("Explorer clicked"))),
        new("Search", Symbol.Search, new RelayCommand(() => notifications.Debug("Search clicked"))),
        new("Source Control", Symbol.BranchFork, new RelayCommand(() => notifications.Debug("Git clicked")))
    ];
}

public record SidebarItem(string Name, StandardIcon Icon, IRelayCommand? Command = null);