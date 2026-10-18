using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkEditor.Workspace.Filesystem.Abstractions;
using SkEditor.Workspace.Projects.Models;

namespace SkEditor.Workspace.Filesystem;

public sealed class DummyFileNode() : FileSystemNode("Loading...", "...", false)
{
    public override char[] InvalidChars => [];
    protected override Func<string, Task<bool>> AlreadyExistsPredicate => _ => Task.FromResult(false);
    protected override Func<string, Task> DeleteAction => _ => Task.CompletedTask;
    protected override Func<string, string, Task> RenameFileAction => (_, _) => Task.CompletedTask;

    public override Task LoadChildrenAsync(bool force = false)
    {
        return Task.CompletedTask;
    }

    public override IEnumerable<ProjectAction> GetSupportedActions()
    {
        return [];
    }

    public override Task RenameAsync(string newName)
    {
        return Task.CompletedTask;
    }

    public override Task<bool> DeleteAsync()
    {
        return Task.FromResult(true);
    }

    public override void UpdateName(string newName)
    {
        Name = newName;
    }

    public override void UpdatePath(string newPath)
    {
        Path = newPath;
    }
}