using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API.Model;
using SkEditor.API.Registry;
using SkEditor.Controls;

namespace SkEditor.Views.Settings;

public partial class ConnectionsPage : UserControl
{
    public ConnectionsPage()
    {
        InitializeComponent();

        AssignCommands();
    }

    public void AssignCommands()
    {
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(HomePage)));
        
        foreach (var connectionData in Registries.Connections)
        {
            ElementsPanel.Children.Add(new ConnectionEntryControl(connectionData));
        }
    }
}