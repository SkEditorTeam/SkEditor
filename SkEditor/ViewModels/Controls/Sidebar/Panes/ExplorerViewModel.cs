using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Projects.Local;
using SkEditor.Models.UI;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Controls.Sidebar.Panes;

public partial class ExplorerViewModel(
    FileTreeViewModel fileTree,
    IFilePickerService filePicker,
    INotificationService notifications) : ObservableObject, ISidebarPanelViewModel
{
    public required FileTreeViewModel FileTree { get; init; } = fileTree;

    public string Name => "Explorer";
    public StandardIcon Icon => Symbol.Folder;

    [RelayCommand]
    public async Task OpenFolder()
    {
        string? path = (await filePicker.PickFoldersAsync(false))?.FirstOrDefault();
        if (path is null) return;
        if (!Directory.Exists(path))
        {
            notifications.ShowError(
                TranslatableString.FromRawString("Oops!"),
                TranslatableString.FromRawString("The selected folder does not exist"));
            return;
        }

        LocalFileSystem localFileSystem = new(path);
        FileTree.FileSystem = localFileSystem;
        await FileTree.LoadFileSystemAsync();
    }
}