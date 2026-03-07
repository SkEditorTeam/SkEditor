using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentIcons.Common;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using SkEditor.API;

namespace SkEditor.Models.Projects;

public class SftpFolderNode(string name, string path, bool isRoot, SftpClient client)
    : FileSystemNode(name, path, true)
{
    public override Symbol Icon => isRoot ? Symbol.Desktop : Symbol.Folder;

    public override async Task LoadChildren()
    {
        // var files = Directory.EnumerateFiles(Path);
        // var folders = Directory.EnumerateDirectories(Path);
        //
        // foreach (var file in files)
        // {
        //     Children.Add(new LocalFileNode(System.IO.Path.GetFileName(file), file));
        // }
        //
        // foreach (var folder in folders)
        // {
        //     LocalFolderNode folderNode = new(System.IO.Path.GetFileName(folder), folder);
        //     Children.Add(folderNode);
        //     await folderNode.LoadChildren();
        // }

        IAsyncEnumerable<ISftpFile> files = client.ListDirectoryAsync(Path, CancellationToken.None);

        await foreach (ISftpFile file in files)
        {
            SkEditorAPI.Logs.Info(
                $"Found file: {file.FullName} (IsDirectory: {file.IsDirectory}, IsRegularFile: {file.IsRegularFile})");

            if (file.IsDirectory)
            {
                if (file.Name is "." or "..")
                {
                    continue;
                }

                SftpFolderNode folderNode = new(file.Name, file.FullName, false, client);
                Children.Add(folderNode);
                await folderNode.LoadChildren();
            }
            else if (file.IsRegularFile)
            {
                Children.Add(new SftpFileNode(file.Name, file.FullName, client));
            }
        }
    }
}