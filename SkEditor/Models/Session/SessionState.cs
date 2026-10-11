using System.Text;
using Avalonia;
using MemoryPack;

namespace SkEditor.Models.Session;

[MemoryPackable]
public partial class SessionState(SessionStateFile[] files)
{
    public SessionStateFile[] Files { get; set; } = files;

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine("SessionState:");
        foreach (SessionStateFile file in Files)
        {
            sb.AppendLine(
                $"  - IsSaved: {file.IsSaved}, Path: {file.Path}, Header: {file.Header}, Content length: {file.Content?.Length ?? -1} characters");
        }

        return sb.ToString();
    }
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