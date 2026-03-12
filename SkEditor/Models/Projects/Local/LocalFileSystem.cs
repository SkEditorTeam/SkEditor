using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Contracts.Models;

namespace SkEditor.Models.Projects.Local;

public class LocalFileSystem : IFileSystem
{
    public LocalFileSystem(string rootDirectory)
    {
        if (!Directory.Exists(rootDirectory))
            throw new DirectoryNotFoundException($"The directory '{rootDirectory}' does not exist.");

        string rootName = Path.GetFileName(rootDirectory);
        Root = new LocalFolderNode(rootName, rootDirectory, true);
    }

    public FileSystemNode? Root { get; }

    public async Task<bool> Load(CancellationToken cancellationToken)
    {
        if (Root == null)
        {
            return false;
        }

        await Root.LoadChildren();
        return true;
    }
}