using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;

namespace SkEditor.Models.Projects;

public abstract partial class FileSystemNode(string name, string path, bool isDirectory) : ObservableObject
{
    [ObservableProperty]
    private bool _isExpanded;

    public string Name { get; set; } = name;
    protected string Path { get; set; } = path;
    public bool IsDirectory { get; set; } = isDirectory;

    public ObservableCollection<FileSystemNode> Children { get; } = [];

    public abstract Symbol Icon { get; }

    protected bool HasLoadedChildren { get; set; }

    public abstract Task LoadChildren();
}