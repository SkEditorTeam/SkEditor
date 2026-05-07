using System.Collections.Generic;
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

    public ObservableCollection<ContextMenuItem> ContextMenuItems =>
        field ??= new ObservableCollection<ContextMenuItem>(GetRawContextMenuItems());

    public abstract Task LoadChildren();

    protected virtual IEnumerable<ContextMenuItem> GetRawContextMenuItems()
    {
        yield break;
    }
}