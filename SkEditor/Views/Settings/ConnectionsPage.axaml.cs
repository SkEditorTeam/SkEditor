using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
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
        foreach (ConnectionData connectionData in Registries.Connections)
        {
            ElementsPanel.Children.Add(new ConnectionEntryControl(connectionData));
        }
    }
}