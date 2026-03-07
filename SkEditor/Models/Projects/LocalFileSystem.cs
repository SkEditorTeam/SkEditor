using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SkEditor.API;
using SkEditor.Contracts.Models;

namespace SkEditor.Models.Projects;

public class LocalFileSystem : IFileSystem
{
    public FileSystemNode Root { get; }

    public LocalFileSystem(string rootDirectory)
    {
        if (!Directory.Exists(rootDirectory))
            throw new DirectoryNotFoundException($"The directory '{rootDirectory}' does not exist.");
        
        string rootName = Path.GetFileName(rootDirectory);
        Root = new LocalFolderNode(rootName, rootDirectory);
    }
    
    public async Task Load()
    {
        await Root.LoadChildren();
    }
}