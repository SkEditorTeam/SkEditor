using System;
using System.IO;
using System.IO.Hashing;
using System.Threading;
using System.Threading.Tasks;

namespace SkEditor.Models.Files;

public abstract class BaseFile(string? path, string? name)
{
    public string? Name { get; set; } = name;
    public string? Path { get; set; } = path;
    public DateTime? LastSaveUtc { get; set; }
    public long Length { get; set; }
    public ulong? DiskHash { get; set; }

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

    protected virtual DateTime? GetLastWriteTimeUtc()
    {
        return null;
    }

    public abstract ValueTask<Stream> LoadAsync();
    public abstract Task SaveAsync(Stream stream, CancellationToken token);
}