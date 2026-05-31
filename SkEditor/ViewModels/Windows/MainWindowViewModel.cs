using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Messages;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Pages;
using TabViewModelBase = SkEditor.ViewModels.Tabs.TabViewModelBase;

namespace SkEditor.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableRecipient, IRecipient<OpenFileMessage>
{
    private readonly IFileService _files;
    private readonly ITabService _tabs;

    public MainWindowViewModel(ITabService tabs, IFileService files, AppSettings appSettings,
        MainMenuViewModel mainMenu,
        SidebarViewModel sidebar,
        StatusBarViewModel statusBar,
        WelcomePageViewModel welcomePage,
        IMessenger messenger)
    {
        _tabs = tabs;
        _files = files;

        AppSettings = appSettings;

        MainMenu = mainMenu;
        WelcomePage = welcomePage;

        _tabs.SelectedTabChanged += tab =>
            SelectedTab = tab;

        Sidebar = sidebar;
        StatusBar = statusBar;

        messenger.Register(this);
    }

    [ObservableProperty]
    public partial TabViewModelBase? SelectedTab { get; set; }

    public AppSettings AppSettings { get; }
    public MainMenuViewModel MainMenu { get; }
    public SidebarViewModel Sidebar { get; }
    public StatusBarViewModel StatusBar { get; }
    public WelcomePageViewModel WelcomePage { get; }

    public ObservableCollection<TabViewModelBase> Tabs => _tabs.Tabs;

    public void Receive(OpenFileMessage message)
    {
        _ = _files.OpenFileAsync(message.File);
    }

    partial void OnSelectedTabChanged(TabViewModelBase? value)
    {
        if (_tabs.SelectedTab != value)
        {
            _tabs.SelectedTab = value;
        }
    }

    [RelayCommand]
    public void NewTab()
    {
        _files.NewFile();
    }
}