using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.DataTemplates;

namespace SkEditor.Views.Controls.StatusBar;

public partial class BottomDrawer : UserControl
{
    public BottomDrawer()
    {
        InitializeComponent();

        DrawerPaneViewLocator locator = App.Services.GetRequiredService<DrawerPaneViewLocator>();
        DataTemplates.Add(locator);
    }
}