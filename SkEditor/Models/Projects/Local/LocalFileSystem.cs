using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Contracts.Models;
using SkEditor.Models.Projects.Types;

namespace SkEditor.Models.Projects.Local;

public partial class LocalFileSystem : IFileSystem
{
    public static readonly char[] InvalidChars =
        Path.GetInvalidFileNameChars();

    public static readonly Func<string, Task<bool>> AlreadyExistsPredicate =
        path => Task.FromResult(File.Exists(path) || Directory.Exists(path));

    public LocalFileSystem(string rootDirectory)
    {
        if (!Directory.Exists(rootDirectory))
        {
            throw new DirectoryNotFoundException($"The directory '{rootDirectory}' does not exist.");
        }

        string rootName = new DirectoryInfo(rootDirectory).Name;
        if (string.IsNullOrEmpty(rootName))
        {
            throw new ArgumentException($"The provided root directory '{rootDirectory}' is invalid.");
        }

        Root = new LocalFolderNode(rootName, rootDirectory, true) { IsRoot = true };

        _watcher = CreateWatcher(rootDirectory);
    }

    public FileSystemNode? Root { get; }

    public async Task<bool> LoadAsync(CancellationToken cancellationToken)
    {
        if (Root == null)
        {
            return false;
        }

        await Root.LoadChildrenAsync();
        return true;
    }
}