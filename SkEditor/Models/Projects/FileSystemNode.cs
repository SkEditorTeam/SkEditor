using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SkEditor.Models.Projects;

public abstract partial class FileSystemNode(string name, string path, bool isDirectory) : ObservableObject
{
    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingChildren { get; set; }

    [ObservableProperty]
    public partial bool IsRenaming { get; set; }

    [ObservableProperty]
    private string _name = name;

    protected string Path { get; set; } = path;
    public bool IsDirectory { get; set; } = isDirectory;
    public bool IsRoot { get; set; }

    public ObservableCollection<FileSystemNode> Children { get; } = [];

    public virtual MenuFlyout? ContextMenu => null;

    public virtual IRelayCommand? OpenCommand => null;

    protected bool HasLoadedChildren { get; set; }

    public abstract Task LoadChildren();
}