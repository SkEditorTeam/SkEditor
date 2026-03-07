using System.Threading.Tasks;
using FluentIcons.Common;
using Renci.SshNet;

namespace SkEditor.Models.Projects;

public class SftpFileNode(string name, string path, SftpClient client) : FileSystemNode(name, path, false)
{
    public override Symbol Icon => Symbol.Document;

    public override Task LoadChildren()
    {
        return Task.CompletedTask;
    }
}