using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkEditor.Models.Files;

public class LocalFile(string? path = null, string? name = null)
    : BaseFile(path, name ?? System.IO.Path.GetFileName(path))
{
    public override ValueTask<Stream> LoadAsync()
    {
        if (Path is null)
        {
            throw new InvalidOperationException("Path cannot be null for LocalFile when loading.");
        }

        Stream stream = new FileStream(
            Path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite,
            4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan
        );

        return ValueTask.FromResult(stream);
    }

    public override async Task SaveAsync(Stream stream, CancellationToken token)
    {
        if (Path is null)
        {
            throw new InvalidOperationException("Path cannot be null for LocalFile when saving.");
        }

        string? directory = System.IO.Path.GetDirectoryName(Path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(Path))
        {
            await File.Create(Path).DisposeAsync();
        }

        string tempFilePath = Path + ".tmp";

        try
        {
            await using (FileStream fileStream = new(tempFilePath, FileMode.Create, FileAccess.Write,
                             FileShare.None, 4096, FileOptions.Asynchronous))
            {
                await stream.CopyToAsync(fileStream, token);
                await fileStream.FlushAsync(token);
            }

            File.Replace(tempFilePath, Path, null);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }
}