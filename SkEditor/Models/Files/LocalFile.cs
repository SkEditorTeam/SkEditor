using System;
using System.IO;
using System.Threading.Tasks;

namespace SkEditor.Models.Files;

public class LocalFile(string name, string? path = null) : BaseFile(name, path)
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
            bufferSize: 4096,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan
        );

        return ValueTask.FromResult(stream);
    }

    public override async Task SaveAsync(Stream stream)
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

        string tempFilePath = Path + ".tmp";

        try
        {
            await using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write,
                             FileShare.None, bufferSize: 4096, options: FileOptions.Asynchronous))
            {
                await stream.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
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