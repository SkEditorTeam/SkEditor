using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using SkEditor.Models.UI;

namespace SkEditor.ViewModels.Controls.Sidebar.Panes;

public partial class ExplorerViewModel(FileTreeViewModel fileTree) : ObservableObject, ISidebarPanelViewModel
{
    public required FileTreeViewModel FileTree { get; init; } = fileTree;

    [ObservableProperty]
    public partial string? FolderPath { get; set; }

    public string Name => "Explorer";
    public StandardIcon Icon => Symbol.Folder;
}