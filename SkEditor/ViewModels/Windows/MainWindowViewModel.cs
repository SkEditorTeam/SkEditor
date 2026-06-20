using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        {
            SelectedTab = tab;
            OnPropertyChanged(nameof(SelectedMainTab));
            OnPropertyChanged(nameof(SelectedSplitTab));
            OnPropertyChanged(nameof(AnyTabsOpen));
            OnPropertyChanged(nameof(IsSplitActive));
        };

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

    public ObservableCollection<TabViewModelBase> MainTabs => _tabs.MainTabs;
    public ObservableCollection<TabViewModelBase> SplitTabs => _tabs.SplitTabs;
    public IEnumerable<TabViewModelBase> AllTabs => _tabs.AllTabs;

    public bool AnyTabsOpen => _tabs.AllTabs.Any();
    public bool IsSplitActive => SplitTabs.Count > 0;

    public TabViewModelBase? SelectedMainTab
    {
        get => _tabs.SelectedMainTab;
        set
        {
            if (_tabs.SelectedMainTab == value)
            {
                return;
            }

            _tabs.SelectedMainTab = value;
            OnPropertyChanged();
        }
    }

    public TabViewModelBase? SelectedSplitTab
    {
        get => _tabs.SelectedSplitTab;
        set
        {
            if (_tabs.SelectedSplitTab == value)
            {
                return;
            }

            _tabs.SelectedSplitTab = value;
            OnPropertyChanged();
        }
    }

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