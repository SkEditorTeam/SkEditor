using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Pages;

namespace SkEditor.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableRecipient
{
    private readonly IFileService _fileService;
    private readonly ITabService _tabService;

    public MainWindowViewModel(ITabService tabService, IFileService fileService, AppSettings appSettings,
        MainMenuViewModel mainMenu,
        WelcomePageViewModel welcomePage)
    {
        _tabService = tabService;
        _fileService = fileService;

        AppSettings = appSettings;

        MainMenu = mainMenu;
        WelcomePage = welcomePage;

        _tabService.SelectedTabChanged += tab =>
            SelectedTab = tab;
    }

    [ObservableProperty]
    public partial Tab? SelectedTab { get; set; }

    public AppSettings AppSettings { get; }
    public MainMenuViewModel MainMenu { get; }
    public WelcomePageViewModel WelcomePage { get; }

    public ObservableCollection<Tab> Tabs => _tabService.Tabs;

    partial void OnSelectedTabChanged(Tab? value)
    {
        if (_tabService.SelectedTab != value)
            _tabService.SelectedTab = value;
    }

    [RelayCommand]
    public void NewTab() => _fileService.NewFile();
}