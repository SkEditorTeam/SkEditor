using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkEditor.Views;
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
            if (e.Key == Key.Escape) Close();
        };

        ColorPicker.ColorChanged += (_, e) =>
        {
            string hex = e.NewColor.ToHexString(false);
            ResultTextBox.Text = hex;
        };

        ResultTextBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter && ResultTextBox.Text.Length == 7)
            {
                bool isValid = Color.TryParse(ResultTextBox.Text, out Color color);
                if (!isValid) return;
                ColorPicker.Color = color;
            }
        };

        CopyButton.Command = new RelayCommand(async () => await Clipboard.SetTextAsync(ResultTextBox.Text));
    }
}
