using SkEditor.Models.Files;

namespace SkEditor.ViewModels.Tabs.Capabilities;

public interface IFileTab
{
    BaseFile File { get; init; }
    string? Path { get; }
}