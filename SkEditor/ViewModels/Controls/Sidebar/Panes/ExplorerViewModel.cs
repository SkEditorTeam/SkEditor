using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Messages;
using SkEditor.Models.Projects.Local;
using SkEditor.Models.UI;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Controls.Sidebar.Panes;

public partial class ExplorerViewModel : ObservableObject, ISidebarPanelViewModel
{
    private readonly IFilePickerService _filePicker;
    private readonly INotificationService _notifications;

    public ExplorerViewModel(FileTreeViewModel fileTree,
        IFilePickerService filePicker,
        INotificationService notifications)
    {
        _filePicker = filePicker;
        _notifications = notifications;
        FileTree = fileTree;

        WeakReferenceMessenger.Default.Register<OpenExplorerFolderMessage>(this, (__, m) =>
        {
            _ = m.FolderPath is not null
                ? OpenFolder(m.FolderPath)
                : OpenFolder();
        });
    }

    public required FileTreeViewModel FileTree { get; init; }

    public string Name => "Explorer";
    public StandardIcon Icon => Symbol.Folder;

    [RelayCommand]
    public async Task OpenFolder()
    {
        string? path = (await _filePicker.PickFoldersAsync(false))?.FirstOrDefault();
        if (path is null) return;
        if (!Directory.Exists(path))
        {
            _notifications.ShowError(
                TranslatableString.FromRawString("Oops!"),
                TranslatableString.FromRawString("The selected folder does not exist"));
            return;
        }

        await OpenFolder(path);
    }

    public async Task OpenFolder(string path)
    {
        LocalFileSystem localFileSystem = new(path);
        FileTree.FileSystem = localFileSystem;
        await FileTree.LoadFileSystemAsync();
    }
}