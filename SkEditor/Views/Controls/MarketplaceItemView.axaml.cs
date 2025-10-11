using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace SkEditor.Views.Controls;

public partial class MarketplaceItemView : UserControl
{
    public static readonly AvaloniaProperty<string> ItemNameProperty =
        AvaloniaProperty.Register<MarketplaceItemView, string>(nameof(ItemName));

    public static readonly AvaloniaProperty<string> ItemVersionProperty =
        AvaloniaProperty.Register<MarketplaceItemView, string>(nameof(ItemVersion));

    public static readonly AvaloniaProperty<string?> CurrentAddonVersionProperty =
        AvaloniaProperty.Register<MarketplaceItemView, string?>(nameof(CurrentAddonVersion));

    public static readonly AvaloniaProperty<string> ItemAuthorProperty =
        AvaloniaProperty.Register<MarketplaceItemView, string>(nameof(ItemAuthor));

    public static readonly AvaloniaProperty<string> ItemImageUrlProperty =
        AvaloniaProperty.Register<MarketplaceItemView, string>(nameof(ItemImageUrl));

    public static readonly AvaloniaProperty<string> ItemShortDescriptionProperty =
        AvaloniaProperty.Register<MarketplaceItemView, string>(nameof(ItemShortDescription));

    public static readonly AvaloniaProperty<string> ItemLongDescriptionProperty =
        AvaloniaProperty.Register<MarketplaceItemView, string>(nameof(ItemLongDescription));


    public MarketplaceItemView()
    {
        InitializeComponent();

        DataContext = this;

        RenderOptions.SetBitmapInterpolationMode(IconImage, BitmapInterpolationMode.HighQuality);
    }

    public string ItemName
    {
        get => GetValue(ItemNameProperty)?.ToString() ?? "";
        set => SetValue(ItemNameProperty, value);
    }

    public string ItemVersion
    {
        get => GetValue(ItemVersionProperty)?.ToString() ?? "";
        set => SetValue(ItemVersionProperty, value);
    }

    public string? CurrentAddonVersion
    {
        get => GetValue(CurrentAddonVersionProperty)?.ToString();
        set => SetValue(CurrentAddonVersionProperty, value);
    }

    public string ItemAuthor
    {
        get => GetValue(ItemAuthorProperty)?.ToString() ?? "";
        set => SetValue(ItemAuthorProperty, value);
    }

    public string ItemImageUrl
    {
        get => GetValue(ItemImageUrlProperty)?.ToString() ?? "";
        set => SetValue(ItemImageUrlProperty, value);
    }

    public string ItemShortDescription
    {
        get => GetValue(ItemShortDescriptionProperty)?.ToString() ?? "";
        set => SetValue(ItemShortDescriptionProperty, value);
    }

    public string ItemLongDescription
    {
        get => GetValue(ItemLongDescriptionProperty)?.ToString() ?? "";
        set => SetValue(ItemLongDescriptionProperty, value);
    }
}