using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.DataTemplates;

namespace SkEditor.Views.Controls.Sidebar;

public partial class SidebarControl : UserControl
{
    public SidebarControl()
    {
        InitializeComponent();

        SidebarPaneViewLocator locator = App.Services.GetRequiredService<SidebarPaneViewLocator>();
        DataTemplates.Add(locator);
    }
}