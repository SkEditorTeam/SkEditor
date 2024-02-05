using Avalonia.Interactivity;
using FluentAvalonia.UI.Windowing;
using SkEditor.Utilities;
using SkEditor.Utilities.Projects.Elements;
using SkEditor.Utilities.Styling;

namespace SkEditor.Views.Projects;

public partial class RenameElementWindow : AppWindow
{
    public StorageElement Element { get; }
    
    public RenameElementWindow(StorageElement element)
    {
        InitializeComponent();

        Element = element;
        NameBox.Text = element.Name;
        
        WindowStyler.Style(this);
    }

    private void RenameButtonClick(object? sender, RoutedEventArgs e)
    {
        var input = NameBox.Text;
        if (string.IsNullOrWhiteSpace(input))
        {
            ErrorBox.Text = Translation.Get("ProjectRenameErrorNameEmpty");
            return;
        }

        var error = Element.ValidateName(input);
        if (error != null)
        {
            ErrorBox.Text = error;
            return;
        }
        
        Element.RenameElement(NameBox.Text);
        Close();
    }

    private void Cancel(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}