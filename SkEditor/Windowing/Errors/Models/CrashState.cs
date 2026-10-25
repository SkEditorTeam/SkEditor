using MemoryPack;

namespace SkEditor.Windowing.Errors.Models;

[MemoryPackable]
public partial class CrashState(CrashStateFile[] files)
{
    public CrashStateFile[] Files { get; set; } = files;
}
