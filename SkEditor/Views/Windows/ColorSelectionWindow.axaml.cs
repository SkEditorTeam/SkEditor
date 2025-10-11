using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;

namespace SkEditor.Views.Windows;

public partial class ColorSelectionWindow : AppWindow
{
    public ColorSelectionWindow()
    {
        InitializeComponent();
        Focusable = true;

        AssignEvents();
    }

    private void AssignEvents()
    {
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };

        ColorPicker.ColorChanged += (_, e) =>
        {
            string hex = e.NewColor.ToHexString(false);
            ResultTextBox.Text = hex;
        };

        ResultTextBox.KeyDown += (_, e) =>
        {
            if (e.Key != Key.Enter || ResultTextBox.Text?.Length != 7)
            {
                return;
            }

            bool isValid = Color.TryParse((string?)ResultTextBox.Text, out Color color);
            if (!isValid)
            {
                return;
            }

            ColorPicker.Color = color;
        };

        CopyButton.Command = new AsyncRelayCommand(async () =>
        {
            if (Clipboard is null)
            {
                return;
            }

            await Clipboard.SetTextAsync(ResultTextBox.Text);
        });
    }
}