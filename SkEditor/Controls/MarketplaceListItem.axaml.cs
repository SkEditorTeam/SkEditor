using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace SkEditor.Controls;
public partial class MarketplaceListItem : UserControl
{
    public static readonly AvaloniaProperty<string> ItemNameProperty = AvaloniaProperty.Register<MarketplaceListItem, string>(nameof(ItemName));
    public static readonly AvaloniaProperty<string> ItemImageUrlProperty = AvaloniaProperty.Register<MarketplaceListItem, string>(nameof(ItemImageUrl));
    public static readonly AvaloniaProperty<string> ItemShortDescriptionProperty = AvaloniaProperty.Register<MarketplaceListItem, string>(nameof(ItemShortDescription));

    public string ItemName
    {
        get => GetValue(ItemNameProperty)?.ToString();
        set => SetValue(ItemNameProperty, value);
    }

    public string ItemImageUrl
    {
        get => GetValue(ItemImageUrlProperty)?.ToString();
        set => SetValue(ItemImageUrlProperty, value);
    }

    public string ItemShortDescription
    {
        get => GetValue(ItemShortDescriptionProperty)?.ToString();
        set => SetValue(ItemShortDescriptionProperty, value);
    }

    public MarketplaceListItem()
    {
        InitializeComponent();

        DataContext = this;

        RenderOptions.SetBitmapInterpolationMode(IconImage, BitmapInterpolationMode.HighQuality);
    }
}
