using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SkEditor.Controls;
public partial class MarketplaceItem : UserControl
{
    public static readonly StyledProperty<string> ItemNameProperty =
        AvaloniaProperty.Register<MarketplaceItem, string>(nameof(ItemName));

    public static readonly StyledProperty<string> IconUrlProperty =
        AvaloniaProperty.Register<MarketplaceItem, string>(nameof(IconUrl));

    public static readonly StyledProperty<string> AuthorProperty =
        AvaloniaProperty.Register<MarketplaceItem, string>(nameof(Author));

    public static readonly StyledProperty<string> VersionProperty =
        AvaloniaProperty.Register<MarketplaceItem, string>(nameof(Version));

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<MarketplaceItem, string>(nameof(Description));

    public string ItemName
    {
        get => GetValue(ItemNameProperty);
        set => SetValue(ItemNameProperty, value);
    }

    public string IconUrl
    {
        get => GetValue(IconUrlProperty);
        set => SetValue(IconUrlProperty, value);
    }

    public string Author
    {
        get => GetValue(AuthorProperty);
        set => SetValue(AuthorProperty, value);
    }

    public string Version
    {
        get => GetValue(VersionProperty);
        set => SetValue(VersionProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public MarketplaceItem()
    {
        InitializeComponent();
        DataContext = this;
    }
}