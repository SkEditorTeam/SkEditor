using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.ComponentModel;

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

    public static readonly StyledProperty<string> LastUpdatedProperty =
        AvaloniaProperty.Register<MarketplaceItem, string>(nameof(LastUpdated));

    public static readonly StyledProperty<string> DownloadsProperty =
        AvaloniaProperty.Register<MarketplaceItem, string>(nameof(Downloads));

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<MarketplaceItem, string>(nameof(Description));

    public static readonly StyledProperty<string> RatingProperty =
        AvaloniaProperty.Register<MarketplaceItem, string>(nameof(Rating));

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

    public string LastUpdated
    {
        get => GetValue(LastUpdatedProperty);
        set => SetValue(LastUpdatedProperty, value);
    }

    public string Downloads
    {
        get => GetValue(DownloadsProperty);
        set => SetValue(DownloadsProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string Rating
    {
        get => GetValue(RatingProperty);
        set => SetValue(RatingProperty, value);
    }

    public MarketplaceItem()
    {
        InitializeComponent();
        DataContext = this;

        Icon.PropertyChanged += OnIconPropertyChanged;
    }

    private void OnIconPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property.Name != nameof(Icon.IsLoading))
            return;

        if (!Icon.IsLoading)
        {
            UpdateIconVisuals();
        }
    }

    private void UpdateIconVisuals()
    {
        IconBorder.Background = Brushes.Transparent;
        Icon.Opacity = 1;
    }
}