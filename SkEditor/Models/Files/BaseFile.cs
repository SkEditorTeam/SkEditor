using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SkEditor.Models.Files;

public abstract class BaseFile(string? path, string? name)
{
    public string? Name { get; set; } = name;
    public string? Path { get; set; } = path;

    public abstract ValueTask<Stream> LoadAsync();
    public abstract Task SaveAsync(Stream stream, CancellationToken token);
}