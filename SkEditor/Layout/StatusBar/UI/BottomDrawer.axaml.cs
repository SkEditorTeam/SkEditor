using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace SkEditor.Layout.StatusBar.UI;

public partial class BottomDrawer : UserControl
{
    public BottomDrawer()
    {
        InitializeComponent();

        DrawerPaneViewLocator locator = App.Services.GetRequiredService<DrawerPaneViewLocator>();
        DataTemplates.Add(locator);
    }
}