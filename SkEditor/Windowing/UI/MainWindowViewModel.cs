using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Configuration.Configs;
using SkEditor.Layout.Menu.Models;
using SkEditor.Layout.Menu.UI;
using SkEditor.Layout.Sidebar.UI;
using SkEditor.Layout.StatusBar.UI;
using SkEditor.Welcome.UI;
using SkEditor.Workspace.Files;
using SkEditor.Workspace.Files.Messages;
using SkEditor.Workspace.Projects.Messages;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Windowing.UI;

public partial class MainWindowViewModel : ObservableRecipient, IRecipient<OpenFileMessage>,
    IRecipient<UpdateFileDetailsMessage>,
    IRecipient<NotifyFileDeleteMessage>
{
    private readonly IFileService _files;
    private readonly ITabService _tabs;

    public IEnumerable<ITab> AllTabs => _tabs.AllTabs;

    public bool AnyTabsOpen => _tabs.AllTabs.Any();

    public AppSettings AppSettings { get; }
    public BottomDrawerViewModel BottomDrawer { get; }
    public bool IsSplitActive => SplitTabs.Count > 0;
    public MainMenuViewModel MainMenu { get; }

    public ObservableCollection<ITab> MainTabs => _tabs.MainTabs;

    public bool ShowInAppMenu =>
        !OperatingSystem.IsMacOS() || AppSettings.UseIntegratedMenuOnMacOs;

    public bool UseThickTitleBar =>
        OperatingSystem.IsMacOS() && AppSettings.UseIntegratedMenuOnMacOs;

    public IEnumerable<MainMenuItem>? NativeMenuItems =>
        OperatingSystem.IsMacOS() && !AppSettings.UseIntegratedMenuOnMacOs ? MainMenu.Items : null;

    public Thickness TitleBarMargin => OperatingSystem.IsMacOS() && AppSettings.UseIntegratedMenuOnMacOs
        ? new Thickness(57, 0, 0, 0)
        : new Thickness(0);

    public double TitleBarHeight
    {
        get
        {
            if (OperatingSystem.IsMacOS())
            {
                return AppSettings.UseIntegratedMenuOnMacOs ? 52 : 24;
            }

            return 50;
        }
    }

    public ITab? SelectedMainTab
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

    public ITab? SelectedSplitTab
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

    [ObservableProperty]
    public partial ITab? SelectedTab { get; set; }

    public SidebarViewModel Sidebar { get; }
    public ObservableCollection<ITab> SplitTabs => _tabs.SplitTabs;
    public StatusBarViewModel StatusBar { get; }
    public WelcomePageViewModel WelcomePage { get; }

    public MainWindowViewModel(ITabService tabs,
        IFileService files,
        AppSettings appSettings,
        MainMenuViewModel mainMenu,
        SidebarViewModel sidebar,
        StatusBarViewModel statusBar,
        BottomDrawerViewModel bottomDrawer,
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
        };

        SplitTabs.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsSplitActive));

        if (_tabs.AllTabs is INotifyCollectionChanged allTabs)
        {
            allTabs.CollectionChanged += (_, _) => OnPropertyChanged(nameof(AllTabs));
        }
        
        Sidebar = sidebar;
        StatusBar = statusBar;
        BottomDrawer = bottomDrawer;

        messenger.Register<OpenFileMessage>(this);
        messenger.Register<UpdateFileDetailsMessage>(this);
        messenger.Register<NotifyFileDeleteMessage>(this);
    }

    public void Receive(OpenFileMessage message)
    {
        _ = _files.OpenFileAsync(message.File);
    }

    public void Receive(UpdateFileDetailsMessage message)
    {
        _files.UpdateFileDetails(message.OldPath, message.NewName, message.NewPath);
    }

    public void Receive(NotifyFileDeleteMessage message)
    {
        _files.NotifyFileDelete(message.Path);
    }

    public void SplitTab(ITab tab)
    {
        _tabs.SplitTab(tab);
    }

    public void MergeTab(ITab tab)
    {
        _tabs.MergeTab(tab);
    }

    [RelayCommand]
    public void NewTab()
    {
        ITab tab = _files.NewFile();
        _tabs.OpenTab(tab);
    }

    partial void OnSelectedTabChanged(ITab? value)
    {
        if (_tabs.SelectedTab != value)
        {
            _tabs.SelectedTab = value;
        }
    }
}