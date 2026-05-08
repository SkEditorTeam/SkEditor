using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;

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

    public abstract Task LoadChildren();

    public abstract List<ContextMenuItem> GetMenuItems();

    public static IEnumerable<ContextMenuItem> GetCommonMenuItems()
    {
        yield return ContextMenuItem.Item("Duplicate").WithIcon(Symbol.DocumentCopy);
        yield return ContextMenuItem.Item("Rename").WithIcon(Symbol.Rename).AllowOneItemOnly();
        yield return ContextMenuItem.Item("Delete").WithIcon(Symbol.Delete);
        yield return ContextMenuItem.Separator;
        yield return ContextMenuItem.Item("Copy Path").WithIcon(Symbol.Clipboard);
        yield return ContextMenuItem.Item("Copy Relative Path").WithIcon(Symbol.ClipboardLink);
    }
}