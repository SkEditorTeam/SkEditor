using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace SkEditor.Views.Settings;
public partial class HomePage : UserControl
{
    public StackPanel GetStackPanel() => ItemStackPanel;

    public HomePage()
    {
        InitializeComponent();

        GeneralItem.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(GeneralPage)));
        PersonalizationItem.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(PersonalizationPage)));
        AboutItem.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(AboutPage)));
        AddonsItem.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(AddonsPage)));
    }
}
