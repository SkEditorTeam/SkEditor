using System.Collections.Generic;

namespace SkEditor.Models.Projects;

public class FileSystemNode(string name, string path, bool isDirectory)
{
    public string Name { get; set; } = name;
    public string Path { get; set; } = path;
    public bool IsDirectory { get; set; } = isDirectory;
    public List<FileSystemNode> Children { get; set; } = [];
}