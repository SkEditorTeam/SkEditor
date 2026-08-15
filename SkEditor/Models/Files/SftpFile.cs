using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet.Common;
using SkEditor.Contracts.Services;
using SkEditor.Models.Remote;

namespace SkEditor.Models.Files;

public class SftpFile(string path, string name, RemoteConnection connection, IRemoteConnectionService service)
    : BaseFile(path, name)
{
    public RemoteConnection Connection => connection;

    public override async ValueTask<Stream> LoadAsync()
    {
        if (Path is null)
        {
            throw new InvalidOperationException("Path cannot be null for SftpFile when loading.");
        }

        try
        {
            return await service.ExecuteSafeAsync(Connection, async client =>
                await client.OpenAsync(Path, FileMode.Open, FileAccess.Read, CancellationToken.None));
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
            await service.ExecuteSafeAsync(Connection, async client =>
                await client.UploadFileAsync(stream, Path, token), token);
        }
        catch (SftpPermissionDeniedException ex)
        {
            throw new UnauthorizedAccessException($"Permission denied when trying to save file at {Path}.", ex);
        }
    }
}