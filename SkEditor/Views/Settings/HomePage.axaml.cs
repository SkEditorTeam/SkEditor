using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using SkEditor.API;

namespace SkEditor.Views.Settings;
public partial class HomePage : UserControl
{
    public StackPanel GetStackPanel() => ItemStackPanel;

    public HomePage()
    {
        InitializeComponent();

        GeneralItem.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(GeneralPage)));
        PersonalizationItem.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(PersonalizationPage)));
        ExperimentsItem.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(ExperimentsPage)));
        AboutItem.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(AboutPage)));
        AddonsItem.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(AddonsPage)));
        ConnectionsItem.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(ConnectionsPage)));
    }
}
