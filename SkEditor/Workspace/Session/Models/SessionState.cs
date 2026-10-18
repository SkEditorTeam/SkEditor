using Avalonia;
using MemoryPack;

namespace SkEditor.Models.Session;

[MemoryPackable]
public partial class SessionState(SessionStateFile[] files)
{
    public SessionStateFile[] Files { get; set; } = files;
    public string? SelectedTabHeader { get; set; }
    public string? OpenFolderPath { get; set; }
    public string? OpenedSidebarPanel { get; set; }
}

[MemoryPackable]
public partial class SessionStateFile
{
    public required bool IsSaved { get; set; }
    public string? Path { get; set; }
    public string? Header { get; set; }
    public string? Content { get; set; }
    public required int CaretOffset { get; set; }
    public required Vector ScrollOffset { get; set; }
}