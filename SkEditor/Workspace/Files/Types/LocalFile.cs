using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace SkEditor.Workspace.Files.Types;

public class LocalFile(string? path = null, string? name = null, IFileSystem? fileSystem = null)
    : BaseFile(path, name ?? System.IO.Path.GetFileName(path))
{
    private static readonly IFileSystem DefaultFileSystem = new FileSystem();
    private readonly IFileSystem _fileSystem = fileSystem ?? DefaultFileSystem;

    public override ValueTask<Stream> LoadAsync()
    {
        if (Path is null)
        {
            throw new InvalidOperationException("Path cannot be null for LocalFile when loading.");
        }

        FileStreamOptions options = new()
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.ReadWrite,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan
        };

        Stream stream = _fileSystem.FileStream.New(Path, options);
        return ValueTask.FromResult(stream);
    }

    public override async Task SaveAsync(Stream stream, CancellationToken token)
    {
        if (Path is null)
        {
            throw new InvalidOperationException("Path cannot be null for LocalFile when saving.");
        }

        string? directory = _fileSystem.Path.GetDirectoryName(Path);
        if (!string.IsNullOrEmpty(directory) && !_fileSystem.Directory.Exists(directory))
        {
            _fileSystem.Directory.CreateDirectory(directory);
        }

        if (!_fileSystem.File.Exists(Path))
        {
            await _fileSystem.File.Create(Path).DisposeAsync();
        }

        string tempFilePath = $"{Path}-{Guid.NewGuid()}.tmp";

        try
        {
            FileStreamOptions options = new()
            {
                Mode = FileMode.Create,
                Access = FileAccess.Write,
                Share = FileShare.None,
                Options = FileOptions.Asynchronous
            };

            await using (FileSystemStream fileStream = _fileSystem.FileStream.New(tempFilePath, options))
            {
                await stream.CopyToAsync(fileStream, token);
                await fileStream.FlushAsync(token);
            }

            _fileSystem.File.Replace(tempFilePath, Path, null);
        }
        finally
        {
            if (_fileSystem.File.Exists(tempFilePath))
            {
                _fileSystem.File.Delete(tempFilePath);
            }
        }
    }

    protected override DateTime? GetLastWriteTimeUtc()
    {
        if (Path is null || !_fileSystem.File.Exists(Path))
        {
            return null;
        }

        return _fileSystem.File.GetLastWriteTimeUtc(Path);
    }
}