using Avalonia;
using Avalonia.Controls;

namespace SkEditor.Controls;
public partial class SettingsTitle : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SettingsTitle, string>(nameof(Title));

    public bool HasBackButton { get; set; } = true;

    public Button GetBackButton() => this.FindControl<Button>("BackButton");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public SettingsTitle()
    {
        InitializeComponent();
        DataContext = this;
    }
}