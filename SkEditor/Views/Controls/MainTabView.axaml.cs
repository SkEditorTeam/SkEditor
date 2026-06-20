using Avalonia.Controls;
using Avalonia.Data.Converters;
using HotAvalonia;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.DataTemplates;

namespace SkEditor.Views.Controls;

public partial class MainTabView : UserControl
{
    public static readonly IValueConverter VisibleToStar =
        new FuncValueConverter<bool, GridLength>(b => b ? new GridLength(1, GridUnitType.Star) : new GridLength(0));

    public static readonly IValueConverter VisibleToAuto =
        new FuncValueConverter<bool, GridLength>(b => b ? GridLength.Auto : new GridLength(0));

    public static readonly IValueConverter VisibleToMinWidth =
        new FuncValueConverter<bool, double>(b => b ? 300 : 0);
    
    public MainTabView()
    {
        InitializeComponent();
        InitializeComponentState();
    }

    [AvaloniaHotReload]
    private void InitializeComponentState()
    {
        TabViewLocator locator = App.Services.GetRequiredService<TabViewLocator>();
        TabView.DataTemplates.Add(locator);
        SplitTabView.DataTemplates.Add(locator);
    }
}