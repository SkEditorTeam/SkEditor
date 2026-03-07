using System.Collections.Generic;
using System.Threading.Tasks;
using FluentIcons.Common;

namespace SkEditor.Models.Projects;

public abstract class FileSystemNode(string name, string path, bool isDirectory)
{
    public string Name { get; set; } = name;
    public string Path { get; set; } = path;
    public bool IsDirectory { get; set; } = isDirectory;
    public List<FileSystemNode> Children { get; set; } = [];
    
    public abstract Symbol Icon { get; }

    public abstract Task LoadChildren();
}