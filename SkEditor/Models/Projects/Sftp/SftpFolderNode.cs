using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace SkEditor.Models.Projects.Sftp;

public class SftpFolderNode(string name, string path, bool isRoot, SftpClient client)
    : BaseFolderNode(name, path, isRoot)
{
    public override bool CanRenameDelete => !IsRoot;
    public override bool CanDownload => true;
    public override bool CanRefresh => IsRoot;
    public override bool CanEditConnection => IsRoot;
    public override bool CanCloseWorkspace => IsRoot;
    public override bool CanDisconnect => IsRoot;

    protected override async Task<IEnumerable<FileSystemNode>> LoadFilesAndFolders()
    {
        List<SftpFolderNode> folders = [];
        List<SftpFileNode> files = [];

        await foreach (ISftpFile file in client.ListDirectoryAsync(Path, CancellationToken.None))
        {
            if (file.IsDirectory)
            {
                if (file.Name is "." or "..")
                {
                    continue;
                }

                folders.Add(new SftpFolderNode(file.Name, file.FullName, false, client));
            }
            else if (file.IsRegularFile)
            {
                files.Add(new SftpFileNode(file.Name, file.FullName, client));
            }
        }

        return folders.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .Concat<FileSystemNode>(files.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase));
    }
}