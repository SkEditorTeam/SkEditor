using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace SkEditor.Layout.Sidebar.UI;

public partial class SidebarControl : UserControl
{
    public SidebarControl()
    {
        InitializeComponent();

        SidebarPaneViewLocator locator = App.Services.GetRequiredService<SidebarPaneViewLocator>();
        DataTemplates.Add(locator);
    }
}