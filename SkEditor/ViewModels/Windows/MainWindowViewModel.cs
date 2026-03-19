using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Models.Tabs;
using SkEditor.ViewModels.Controls;

namespace SkEditor.ViewModels.Windows;

public partial class MainWindowViewModel(MainMenuViewModel mainMenu) : ObservableObject
{
    public MainMenuViewModel MainMenu { get; } = mainMenu;

    [ObservableProperty]
    private ObservableCollection<Tab> _tabs = [];

    public Tab? SelectedTab { get; set; }
}