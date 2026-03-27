using System.IO;
using System.Threading.Tasks;

namespace SkEditor.Models.Files;

public abstract class BaseFile(string name, string? path)
{
    public string? Path { get; } = path;
    public string Name { get; set; } = name;
    public bool IsSaved { get; set; } = false;

    public abstract ValueTask<Stream> LoadAsync();
    public abstract Task SaveAsync(Stream stream);
}