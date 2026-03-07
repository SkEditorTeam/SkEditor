using System.IO;
using System.Threading.Tasks;
using FluentIcons.Common;

namespace SkEditor.Models.Projects;

public class LocalFolderNode(string name, string path) : FileSystemNode(name, path, true)
{
    public override Symbol Icon { get; } = Symbol.Folder;
    
    public override async Task LoadChildren()
    {
        var files = Directory.EnumerateFiles(Path);
        var folders = Directory.EnumerateDirectories(Path);
        
        foreach (var file in files)
        {
            Children.Add(new LocalFileNode(System.IO.Path.GetFileName(file), file));
        }
        
        foreach (var folder in folders)
        {
            LocalFolderNode folderNode = new(System.IO.Path.GetFileName(folder), folder);
            Children.Add(folderNode);
            await folderNode.LoadChildren();
        }
    }
}