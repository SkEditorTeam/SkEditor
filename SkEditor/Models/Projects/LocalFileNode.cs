using System.Threading.Tasks;
using FluentIcons.Common;

namespace SkEditor.Models.Projects;

public class LocalFileNode(string name, string path) : FileSystemNode(name, path, false)
{
    public override Symbol? Icon => Symbol.Document;

    public override Task LoadChildren()
    {
        return Task.CompletedTask;
    }
}