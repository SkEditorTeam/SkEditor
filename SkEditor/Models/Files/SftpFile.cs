using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using SkEditor.Models.Remote;

namespace SkEditor.Models.Files;

public class SftpFile(string path, string name, RemoteConnection connection, SftpClient client) : BaseFile(path, name)
{
    public RemoteConnection Connection { get; } = connection;
    
    public override async ValueTask<Stream> LoadAsync()
    {
        if (Path is null)
        {
            throw new InvalidOperationException("Path cannot be null for SftpFile when loading.");
        }

        try
        {
            SftpFileStream stream =
                await client.OpenAsync(Path, FileMode.Open, FileAccess.Read, CancellationToken.None);
            return stream;
        }
        catch (SftpPermissionDeniedException ex)
        {
            throw new UnauthorizedAccessException($"Permission denied when trying to load file at {Path}.", ex);
        }
    }

    public override async Task SaveAsync(Stream stream, CancellationToken token)
    {
        if (Path is null)
        {
            throw new InvalidOperationException("Path cannot be null for SftpFile when saving.");
        }

        stream.Position = 0;

        try
        {
            await client.UploadFileAsync(stream, Path, token);
        }
        catch (SftpPermissionDeniedException ex)
        {
            throw new UnauthorizedAccessException($"Permission denied when trying to save file at {Path}.", ex);
        }
    }
}