using System.IO;
using System.Threading.Tasks;
using R3;
using SkEditor.Workspace.Files.Types;

namespace SkEditor.Workspace.Tabs.Capabilities;

public interface IFileTab : ITab, ISaveableTab, IClosableTab
{
    BaseFile File { get; }
    string? Path { get; }

    bool? HasPath => !string.IsNullOrEmpty(Path);
    Observable<string?> PathChanged { get; }
    void ChangeFile(BaseFile file);

    Task UpdateDocumentText(string text);
    void UpdateHeader();

    void WriteContentStream(StreamWriter writer);
}