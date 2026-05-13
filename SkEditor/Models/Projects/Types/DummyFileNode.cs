using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkEditor.Models.Projects.Types;

public sealed class DummyFileNode() : FileSystemNode("Loading...", "...", false)
{
    protected override Func<string, Task<bool>> AlreadyExistsPredicate => _ => Task.FromResult(false);
    protected override Func<string, Task> DeleteAction => _ => Task.CompletedTask;
    protected override Func<string, string, Task> RenameFileAction => (_, _) => Task.CompletedTask;
    public override char[] InvalidChars => [];

    public override Task LoadChildrenAsync()
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<ProjectAction> GetSupportedActions()
    {
        throw new NotImplementedException();
    }

    public override Task RenameAsync(string newName)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> DeleteAsync()
    {
        throw new NotImplementedException();
    }
}