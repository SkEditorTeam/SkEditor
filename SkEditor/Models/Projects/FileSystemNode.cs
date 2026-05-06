using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
    public partial string Name { get; set; } = name;

    public string Path { get; set; } = path;
    public bool IsDirectory { get; set; } = isDirectory;
    public bool IsRoot { get; set; }

    public ObservableCollection<FileSystemNode> Children { get; } = [];

    public virtual IRelayCommand? OpenCommand => null;

    protected bool HasLoadedChildren { get; set; }

    public virtual bool CanOpen => false;
    public virtual bool CanOpenInExplorer => false;

    public abstract Task LoadChildren();
}