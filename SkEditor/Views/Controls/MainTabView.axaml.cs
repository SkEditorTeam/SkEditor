using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.DataTemplates;

namespace SkEditor.Views.Controls;

public partial class MainTabView : UserControl
{
    public MainTabView()
    {
        InitializeComponent();

        TabViewLocator locator = App.Services.GetRequiredService<TabViewLocator>();
        TabView.DataTemplates.Add(locator);
    }
}