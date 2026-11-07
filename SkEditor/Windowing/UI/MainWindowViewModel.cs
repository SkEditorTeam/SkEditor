using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Configuration.Configs;
using SkEditor.Core.System;
using SkEditor.Layout.Menu.Models;
using SkEditor.Layout.Menu.UI;
using SkEditor.Layout.Sidebar.UI;
using SkEditor.Layout.StatusBar.UI;
using SkEditor.Welcome.UI;
using SkEditor.Workspace.Files.Messages;
using SkEditor.Workspace.Projects.Messages;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Windowing.UI;

public partial class MainWindowViewModel : ObservableRecipient, IRecipient<OpenFileMessage>,
    IRecipient<UpdateFileDetailsMessage>,
    IRecipient<NotifyFileDeleteMessage>
{
    private readonly IFileTabService _fileTabs;
    private readonly ITabService _tabs;
    private readonly ILinkService _links;
    private readonly IClipboardService _clipboard;

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
        IFileTabService fileTabs,
        ILinkService links,
        IClipboardService clipboard,
        AppSettings appSettings,
        MainMenuViewModel mainMenu,
        SidebarViewModel sidebar,
        StatusBarViewModel statusBar,
        BottomDrawerViewModel bottomDrawer,
        WelcomePageViewModel welcomePage,
        IMessenger messenger)
    {
        _tabs = tabs;
        _fileTabs = fileTabs;
        _links = links;
        _clipboard = clipboard;

        AppSettings = appSettings;

        MainMenu = mainMenu;
        WelcomePage = welcomePage;

        this._tabs.SelectedTabChanged += tab =>
        {
            SelectedTab = tab;
            OnPropertyChanged(nameof(SelectedMainTab));
            OnPropertyChanged(nameof(SelectedSplitTab));
            OnPropertyChanged(nameof(AnyTabsOpen));
        };

        SplitTabs.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsSplitActive));

        if (this._tabs.AllTabs is INotifyCollectionChanged allTabs)
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
        _ = _fileTabs.OpenFileAsync(message.File);
    }

    public void Receive(UpdateFileDetailsMessage message)
    {
        _fileTabs.UpdateFileDetails(message.OldPath, message.NewName, message.NewPath);
    }

    public void Receive(NotifyFileDeleteMessage message)
    {
        _fileTabs.NotifyFileDelete(message.Path);
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
    public async Task CloseTab(ITab tab)
    {
        await _tabs.CloseTabAsync(tab);
    }

    [RelayCommand]
    public void NewTab()
    {
        ITab tab = _fileTabs.NewFile();
        _tabs.OpenTab(tab);
    }
    
    [RelayCommand]
    private void OpenInExplorer(ITab tab)
    {
        if (tab is not IFileTab fileTab) return;
        
        if (fileTab.Path is not null)
        {
            _links.OpenFolderWithFileSelection(fileTab.Path);
        }
    }

    [RelayCommand]
    private async Task CopyName(ITab tab)
    {
        if (tab is not IFileTab) return;
        
        await _clipboard.CopyTextAsync(tab.Header.Value);
    }

    [RelayCommand]
    private async Task CopyPath(ITab tab)
    {
        if (tab is not IFileTab fileTab) return;
        
        if (fileTab.Path is not null)
        {
            string pathToCopy = OperatingSystem.IsWindows() && AppSettings.IsExplorerForwardSlashesCopyEnabled
                ? fileTab.Path.Replace('\\', '/')
                : fileTab.Path;

            await _clipboard.CopyTextAsync(pathToCopy);
        }
    }

    partial void OnSelectedTabChanged(ITab? value)
    {
        if (_tabs.SelectedTab != value)
        {
            _tabs.SelectedTab = value;
        }
    }
}