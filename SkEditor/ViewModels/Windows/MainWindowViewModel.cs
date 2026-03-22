using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Models.Tabs;
using SkEditor.ViewModels.Controls;

namespace SkEditor.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableRecipient
{
    [ObservableProperty]
    private Tab? _selectedTab;

    [ObservableProperty]
    private ObservableCollection<Tab> _tabs = [];

    public MainWindowViewModel(MainMenuViewModel mainMenu)
    {
        MainMenu = mainMenu;

        Tabs.CollectionChanged += (_, e) =>
        {
            if (SelectedTab is null && Tabs.Count > 0)
                Dispatcher.UIThread.Post(() => SelectedTab = Tabs[0]);
        };

        Messenger.Register<TabCloseRequestedMessage>(this, (r, m) =>
            Tabs.Remove(m.Tab));
    }

    public MainMenuViewModel MainMenu { get; }

    [RelayCommand]
    public void NewTab()
    {
        // Tabs.Add(new EditorTab("New Tab", "new tab"));
        Tab tab = new SettingsTab();
        Tabs.Add(tab);
        SelectedTab = tab;
    }
}