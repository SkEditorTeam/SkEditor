using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Windowing;
using SkEditor.Utilities.Projects.Elements;
using SkEditor.Utilities.Styling;

namespace SkEditor.Views.Projects;

public partial class CreateStorageElementWindow : AppWindow
{
    public Folder Folder;
    public bool IsFile;
    
    public CreateStorageElementWindow(Folder folder, bool isFile)
    {
        Folder = folder;
        IsFile = isFile;
        
        InitializeComponent();
        WindowStyler.Style(this);
    }
}