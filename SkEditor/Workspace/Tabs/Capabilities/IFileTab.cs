using CommunityToolkit.Mvvm.Input;
using SkEditor.Models.Files;

namespace SkEditor.ViewModels.Tabs.Capabilities;

public interface IFileTab
{
    BaseFile File { get; init; }
    string? Path { get; }

    bool? HasPath => !string.IsNullOrEmpty(Path);

    IRelayCommand OpenInExplorerRelayCommand { get; }
    IRelayCommand CopyNameRelayCommand { get; }
    IRelayCommand CopyPathRelayCommand { get; }
}