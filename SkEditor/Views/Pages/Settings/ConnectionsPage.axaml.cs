using Avalonia.Controls;
using SkEditor.API;
using ConnectionEntryControl = SkEditor.Views.Controls.ConnectionEntryControl;

namespace SkEditor.Views.Pages.Settings;

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