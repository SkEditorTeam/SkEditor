using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SettingsWindow = SkEditor.Views.Windows.Settings.SettingsWindow;

namespace SkEditor.Views.Controls;

public partial class SettingsTitle : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SettingsTitle, string>(nameof(Title));

    public SettingsTitle()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            BackButton.Command = new RelayCommand(SettingsWindow.Instance.FrameView.GoBack);
        };
    }

    public bool HasBackButton { get; set; } = true;

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets the back button control.
    /// </summary>
    /// <returns>The back button control, or null if it does not exist.</returns>
    public Button? GetBackButton()
    {
        return this.FindControl<Button>("BackButton");
    }
}