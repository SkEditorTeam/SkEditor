using System.Threading.Tasks;

namespace SkEditor.Models.Projects;

public abstract class BaseFileNode(string name, string path) : FileSystemNode(name, path, false)
{
    public override Task LoadChildren()
    {
        return Task.CompletedTask;
    }
}