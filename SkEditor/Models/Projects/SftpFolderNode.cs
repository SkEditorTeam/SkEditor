using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using FluentIcons.Common;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace SkEditor.Models.Projects;

public class SftpFolderNode : FileSystemNode
{
    private static readonly FileSystemNode DummyNode = new LocalFileNode("Loading...", "...");
    private readonly SftpClient _client;
    private readonly bool _isRoot;

    public SftpFolderNode(string name, string path, bool isRoot, SftpClient client) : base(name, path, true)
    {
        _isRoot = isRoot;
        _client = client;

        if (isRoot)
        {
            return;
        }

        Children.Add(DummyNode);
    }

    public override Symbol Icon => _isRoot ? Symbol.Desktop : Symbol.Folder;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(IsExpanded) && IsExpanded && !HasLoadedChildren)
        {
            _ = LoadChildren();
        }
    }

    public override async Task LoadChildren()
    {
        if (HasLoadedChildren)
        {
            return;
        }

        Children.Clear();

        IAsyncEnumerable<ISftpFile> files = _client.ListDirectoryAsync(Path, CancellationToken.None);

        await foreach (ISftpFile file in files)
        {
            if (file.IsDirectory)
            {
                if (file.Name is "." or "..")
                {
                    continue;
                }

                SftpFolderNode folderNode = new(file.Name, file.FullName, false, _client);
                Children.Add(folderNode);
            }
            else if (file.IsRegularFile)
            {
                Children.Add(new SftpFileNode(file.Name, file.FullName, _client));
            }
        }

        HasLoadedChildren = true;
    }
}