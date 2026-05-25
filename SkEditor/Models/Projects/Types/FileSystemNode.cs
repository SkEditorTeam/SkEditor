using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SkEditor.Models.Projects.Types;

public abstract partial class FileSystemNode(string name, string path, bool isDirectory) : ObservableObject
{
    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingChildren { get; set; }

    [ObservableProperty]
    public partial bool IsRenaming { get; set; }

    [ObservableProperty]
    public partial string Name { get; protected set; } = name;

    public string Path { get; protected set; } = path;
    public bool IsDirectory { get; set; } = isDirectory;
    public bool IsRoot { get; init; }

    public ObservableCollection<FileSystemNode> Children { get; } = [];

    public virtual IRelayCommand? OpenCommand => null;

    protected abstract Func<string, Task<bool>> AlreadyExistsPredicate { get; }

    protected abstract Func<string, Task> DeleteAction { get; }
    protected abstract Func<string, string, Task> RenameFileAction { get; }

    public abstract char[] InvalidChars { get; }

    protected bool HasLoadedChildren { get; set; }

    public abstract Task LoadChildrenAsync(bool force = false);

    public abstract IEnumerable<ProjectAction> GetSupportedActions();

    public event EventHandler<NodeRenamedEventArgs>? Renamed;
    public event EventHandler? Deleted;

    public void OnRenamed(string oldName, string newName)
    {
        Renamed?.Invoke(this, new NodeRenamedEventArgs(oldName, newName));
    }

    public void OnDeleted()
    {
        Deleted?.Invoke(this, EventArgs.Empty);
    }

    public abstract Task RenameAsync(string newName);
    public abstract Task<bool> DeleteAsync();

    public abstract void UpdatePath(string newPath);
}

public class NodeRenamedEventArgs(string oldName, string newName) : EventArgs
{
    public string OldName { get; } = oldName;
    public string NewName { get; } = newName;
}