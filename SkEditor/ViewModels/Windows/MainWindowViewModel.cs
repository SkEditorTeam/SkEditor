using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Tabs;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Pages;

namespace SkEditor.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableRecipient
{
    private readonly ITabService _tabService;

    [ObservableProperty]
    private Tab? _selectedTab;

    public MainWindowViewModel(ITabService tabService, MainMenuViewModel mainMenu, WelcomePageViewModel welcomePage)
    {
        _tabService = tabService;
        MainMenu = mainMenu;
        WelcomePage = welcomePage;

        _tabService.SelectedTabChanged += tab =>
            Dispatcher.UIThread.Post(() => SelectedTab = tab);
    }

    public MainMenuViewModel MainMenu { get; }
    public WelcomePageViewModel WelcomePage { get; }

    public ObservableCollection<Tab> Tabs => _tabService.Tabs;

    partial void OnSelectedTabChanged(Tab? value)
    {
        if (_tabService.SelectedTab != value)
            _tabService.SelectedTab = value;
    }

    [RelayCommand]
    public void NewTab() => _tabService.OpenTab(new SettingsTab());
}