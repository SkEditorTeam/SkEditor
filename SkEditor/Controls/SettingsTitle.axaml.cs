using Avalonia;
using Avalonia.Controls;

namespace SkEditor.Controls;

public partial class SettingsTitle : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SettingsTitle, string>(nameof(Title));

    public SettingsTitle()
    {
        InitializeComponent();
        DataContext = this;
    }

    public bool HasBackButton { get; set; } = true;

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public Button? GetBackButton()
    {
        return this.FindControl<Button>("BackButton");
    }
}