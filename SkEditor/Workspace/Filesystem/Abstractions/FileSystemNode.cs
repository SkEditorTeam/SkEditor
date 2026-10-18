using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SkEditor.Models.Projects.Types;

public abstract partial class FileSystemNode(string name, string path, bool isDirectory) : ObservableObject
{
    public ObservableCollection<FileSystemNode> Children { get; } = [];

    public bool HasLoadedChildren { get; set; }

    public abstract char[] InvalidChars { get; }
    public bool IsDirectory { get; set; } = isDirectory;

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingChildren { get; set; }

    [ObservableProperty]
    public partial bool IsRenaming { get; set; }

    public bool IsRoot { get; init; }

    [ObservableProperty]
    public partial string Name { get; protected set; } = name;

    public virtual IRelayCommand? OpenCommand => null;

    public string Path { get; protected set; } = path;

    protected abstract Func<string, Task<bool>> AlreadyExistsPredicate { get; }

    protected abstract Func<string, Task> DeleteAction { get; }
    protected abstract Func<string, string, Task> RenameFileAction { get; }

    public abstract Task LoadChildrenAsync(bool force = false);

    public abstract IEnumerable<ProjectAction> GetSupportedActions();

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

    public abstract void UpdateName(string newName);
    public abstract void UpdatePath(string newPath);

    protected virtual string CombinePath(string directory, string name)
    {
        return System.IO.Path.Combine(directory, name);
    }

    protected virtual string GetDirectoryName(string path)
    {
        return System.IO.Path.GetDirectoryName(path) ?? string.Empty;
    }

    public event EventHandler<NodeRenamedEventArgs>? Renamed;
    public event EventHandler? Deleted;
}

public class NodeRenamedEventArgs(string oldName, string newName) : EventArgs
{
    public string NewName { get; } = newName;
    public string OldName { get; } = oldName;
}