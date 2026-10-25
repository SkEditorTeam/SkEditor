using MemoryPack;

namespace SkEditor.Windowing.Errors.Models;

[MemoryPackable]
public partial class CrashStateFile
{
    public required bool IsSaved { get; set; }
    public string? Path { get; set; }
    public string? Header { get; set; }
    public string? Content { get; set; }
    public bool IsRemote { get; set; }
}