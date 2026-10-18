using System;
using System.IO;
using System.IO.Hashing;
using System.Threading;
using System.Threading.Tasks;

namespace SkEditor.Models.Files;

/// <summary>
///     Represents a file in a file tab, having basic properties like name, path and disk snapshot properties.
/// </summary>
/// <param name="path">Path to the file on file system.</param>
/// <param name="name">Name of the file.</param>
public abstract class BaseFile(string? path, string? name)
{
    /// <summary>
    ///     Name of the file.
    ///     <remarks>Null if the file is not saved yet.</remarks>
    /// </summary>
    public string? Name { get; set; } = name;

    /// <summary>
    ///     Path to the file on file system.
    ///     <remarks>Null if the file is not saved yet.</remarks>
    /// </summary>
    public string? Path { get; set; } = path;

    /// <summary>
    ///     The last time the file was saved to disk, UTC.
    ///     <remarks>Uses the OS-provided last write time. It's always null for non-local files.</remarks>
    /// </summary>
    public DateTime? LastSaveUtc { get; set; }

    /// <summary>
    ///     The length of the file on disk, in bytes.
    ///     <remarks>Gets updated only when the file is loaded or saved.</remarks>
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    ///     The hash of the file on disk, using xxHash3.
    ///     <remarks>Gets updated only when the file is loaded or saved.</remarks>
    /// </summary>
    public ulong? DiskHash { get; set; }

    /// <summary>
    ///     Updates the snapshot properties of the file (Length, LastSaveUtc, DiskHash) based on the provided stream.
    ///     <remarks>The stream position is reset to 0 before and after the operation.</remarks>
    /// </summary>
    public async Task UpdateSnapshotAsync(Stream stream)
    {
        stream.Position = 0;
        Length = stream.Length;

        LastSaveUtc = GetLastWriteTimeUtc();

        XxHash3 hasher = new();
        await hasher.AppendAsync(stream);
        DiskHash = hasher.GetCurrentHashAsUInt64();
        stream.Position = 0;
    }

    /// <summary>
    ///     Gets the last write time of the file on disk, UTC.
    /// </summary>
    protected virtual DateTime? GetLastWriteTimeUtc()
    {
        return null;
    }

    /// <summary>
    ///     Loads the file and returns a stream to read its contents.
    /// </summary>
    public abstract ValueTask<Stream> LoadAsync();

    /// <summary>
    ///     Saves the file from the provided stream.
    /// </summary>
    public abstract Task SaveAsync(Stream stream, CancellationToken token);
}