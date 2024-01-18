using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace SkEditor.Views.FileTypes;

public partial class ImageViewer : UserControl
{
    
    public ImageViewer(Bitmap image, string path)
    {
        InitializeComponent();

        Image.Source = path;
        Image.Image = image;
        InformationText.Text = $"{Path.GetExtension(path).ToUpper()} Image ({image.Size.Width}x{image.Size.Height} pixels)";
        
        AssignCommands();
    }
    
    private void AssignCommands()
    {
        AntialiasingModeToggle.IsCheckedChanged += AntialiasingModeToggleOnChecked;
    }
    
    private void AntialiasingModeToggleOnChecked(object? sender, RoutedEventArgs e)
    {
        RenderOptions.SetBitmapInterpolationMode(Image, AntialiasingModeToggle.IsChecked == true
            ? BitmapInterpolationMode.HighQuality
            : BitmapInterpolationMode.None);
        Image.InvalidateVisual();
    }
    
}