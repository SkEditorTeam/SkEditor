using Avalonia.Input;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Projects.Elements;
using SkEditor.Utilities.Styling;

namespace SkEditor.Views.Projects;

public partial class RenameElementWindow : AppWindow
{
    public RenameElementWindow(StorageElement element)
    {
        InitializeComponent();
        Focusable = true;

        Element = element;
        NameBox.Text = element.Name;

        WindowStyler.Style(this);

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };
    }

    public StorageElement Element { get; }

    private void RenameButtonClick(object? sender, RoutedEventArgs e)
    {
        string? input = NameBox.Text;
        if (string.IsNullOrWhiteSpace(input))
        {
            ErrorBox.Text = Translation.Get("ProjectRenameErrorNameEmpty");
            return;
        }

        string? error = Element.ValidateName(input);
        if (error != null)
        {
            ErrorBox.Text = error;
            return;
        }

        Element.RenameElement(input);
        Close();
    }

    private void Cancel(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    
    
}