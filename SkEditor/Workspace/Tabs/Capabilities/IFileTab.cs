using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Workspace.Files.Types;

namespace SkEditor.Workspace.Tabs.Capabilities;

public interface IFileTab : ITab, ISaveableTab, IClosableTab
{
    BaseFile File { get; init; }
    string? Path { get; }

    bool? HasPath => !string.IsNullOrEmpty(Path);

    Task UpdateDocumentText(string text);
    void UpdateHeader();

    void WriteContentStream(StreamWriter writer);

    IRelayCommand OpenInExplorerRelayCommand { get; }
    IRelayCommand CopyNameRelayCommand { get; }
    IRelayCommand CopyPathRelayCommand { get; }
}