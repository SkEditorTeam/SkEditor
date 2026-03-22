using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Models.Tabs;
using SkEditor.ViewModels.Controls;

namespace SkEditor.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableRecipient
{
    public MainMenuViewModel MainMenu { get; }

    [ObservableProperty]
    private ObservableCollection<Tab> _tabs = [];
    
    [ObservableProperty]
    private Tab? _selectedTab;
    
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

    [RelayCommand]
    public void NewTab()
    {
        Tabs.Add(new EditorTab("New Tab", new TextEditor {Background = Brush.Parse("#240B2E")}));
    }
}