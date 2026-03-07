using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Models.Projects;

namespace SkEditor.ViewModels.Controls;

public partial class FileTreeViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<FileSystemNode> _nodes;
    
    public FileTreeViewModel()
    {
        var rootNode = new FileSystemNode("Root", "C:\\Root", true);
        rootNode.Children.Add(new FileSystemNode("File1.txt", @"C:\Root\File1.txt", false));
        rootNode.Children.Add(new FileSystemNode("File2.txt", @"C:\Root\File2.txt", false));
        
        Nodes = [rootNode];
    }
}