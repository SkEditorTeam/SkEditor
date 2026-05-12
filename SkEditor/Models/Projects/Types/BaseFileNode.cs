using System.Threading.Tasks;

namespace SkEditor.Models.Projects.Types;

public abstract class BaseFileNode(string name, string path) : FileSystemNode(name, path, false)
{
    public override Task LoadChildrenAsync()
    {
        return Task.CompletedTask;
    }
}