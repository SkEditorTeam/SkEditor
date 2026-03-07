using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentIcons.Common;

namespace SkEditor.Models.Projects;

public abstract class FileSystemNode(string name, string path, bool isDirectory)
{
    public string Name { get; set; } = name;
    protected string Path { get; set; } = path;
    public bool IsDirectory { get; set; } = isDirectory;

    public ObservableCollection<FileSystemNode> Children { get; } = [];

    public abstract Symbol Icon { get; }

    public abstract Task LoadChildren();
}