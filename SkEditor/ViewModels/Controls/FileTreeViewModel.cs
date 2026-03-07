using System.Collections.Generic;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.API;
using SkEditor.Contracts.Models;
using SkEditor.Models.Projects;

namespace SkEditor.ViewModels.Controls;

public partial class FileTreeViewModel : ObservableObject
{
    [ObservableProperty]
    private IFileSystem _fileSystem;
    
    public IEnumerable<FileSystemNode> RootItems => [FileSystem.Root];
    
    public FileTreeViewModel()
    {
        _fileSystem = new LocalFileSystem(@"C:\Users\Notro\Documents");
     
        Stopwatch stopwatch = Stopwatch.StartNew();
        _fileSystem.Load();
        stopwatch.Stop();
        SkEditorAPI.Logs.Info($"Loaded file system in {stopwatch.ElapsedMilliseconds} ms");
    }
}