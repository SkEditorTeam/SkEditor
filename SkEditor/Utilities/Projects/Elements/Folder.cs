using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Files;
using static SkEditor.Views.Controls.Sidebar.ExplorerSidebarPanel;
using CreateStorageElementWindow = SkEditor.Views.Windows.Projects.CreateStorageElementWindow;
using MainWindow = SkEditor.Views.Windows.MainWindow;

namespace SkEditor.Utilities.Projects.Elements;

public partial class Folder : StorageElement
{
    public Folder(string folder, Folder? parent = null)
    {
        folder = Uri.UnescapeDataString(folder)
            .NormalizePathSeparators()
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        Parent = parent;
        StorageFolderPath = folder;

        if (folder.StartsWith(@"\\") && folder.Count(c => c == '\\') == 3)
        {
            string[] parts = folder.Split('\\', StringSplitOptions.RemoveEmptyEntries);

            Name = parts.Length >= 2 ? parts[1] : Path.GetFileName(folder);
        }
        else
        {
            Name = Path.GetFileName(folder);
        }

        if (string.IsNullOrEmpty(Name))
        {
            string[] segments = folder.Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries
            );

            Name = segments.Length > 0 ? segments[^1] : "Root";
        }

        IsFile = false;
        IsRootFolder = parent is null;
        Children = [];

        _ = LoadChildren();

        OpenInExplorerCommand = new RelayCommand(OpenInExplorer);
        DeleteCommand = new AsyncRelayCommand(DeleteFolder);
        CopyPathCommand = new RelayCommand(CopyPath);
        CopyAbsolutePathCommand = new RelayCommand(CopyAbsolutePath);
        CreateNewFileCommand = new AsyncRelayCommand(() => CreateNewElement(true));
        CreateNewFolderCommand = new AsyncRelayCommand(() => CreateNewElement(false));
        CloseProjectCommand = new RelayCommand(CloseProject);
    }

    public string StorageFolderPath { get; set; }

    private async Task LoadChildren()
    {
        await Task.Run(() =>
            Dispatcher.UIThread.Invoke(() =>
            {
                Directory
                    .GetDirectories(StorageFolderPath)
                    .ToList()
                    .ForEach(x => Children?.Add(new Folder(x, this)));

                Directory
                    .GetFiles(StorageFolderPath)
                    .ToList()
                    .ForEach(x => Children?.Add(new File(x, this)));
            })
        );
    }

    public void OpenInExplorer()
    {
        Process.Start(new ProcessStartInfo(StorageFolderPath) { UseShellExecute = true });
    }

    public async Task DeleteFolder()
    {
        ContentDialogResult result = await SkEditorAPI.Windows.ShowDialog(
            Translation.Get("DeleteFolderTitle"),
            Translation.Get("DeleteStorageElement", Name),
            Symbol.Delete,
            primaryButtonText: "DeleteButton",
            cancelButtonText: "CancelButton", translate: true
        );

        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        Directory.Delete(StorageFolderPath, true);
        Parent?.Children?.Remove(this);

        if (Parent is null)
        {
            CloseProject();
        }
    }

    private static void CloseProject()
    {
        ProjectOpener.FileTreeView.ItemsSource = null;

        Folder? projectRootFolder = null;
        
        SkEditorAPI.Events.ProjectClosed();

        ExplorerPanel? panel =
            Registries.SidebarPanels.FirstOrDefault(x => x is ExplorerPanel) as ExplorerPanel;

        StackPanel? noFolderMessage = panel?.Panel.NoFolderMessage;
        if (noFolderMessage is null) return;
        noFolderMessage.IsVisible = projectRootFolder == null;
    }

    public override void RenameElement(string newName, bool move = true)
    {
        Folder? parent = Parent;
        if (parent is null) return;
        string newPath = Path.Combine(parent.StorageFolderPath, newName);

        if (move)
        {
            Directory.Move(StorageFolderPath, newPath);
        }

        StorageFolderPath = newPath;
        Name = newName;
        RefreshSelf();
    }

    public override string? ValidateName(string input)
    {
        if (input == Name)
        {
            return Translation.Get("ProjectRenameErrorSameName");
        }

        if (Parent is null)
        {
            return Translation.Get("ProjectRenameErrorParentNull");
        }

        if (!ValidFolderNameRegex().IsMatch(input))
        {
            return Translation.Get("ProjectRenameErrorNameInvalid");
        }
        
        StorageElement? folder = Parent?.Children?.FirstOrDefault(x => x.Name == input);

        return folder is not null ? Translation.Get("ProjectErrorNameExists") : null;
    }

    public string? ValidateCreationName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Translation.Get("ProjectCreateErrorNameEmpty");
        }
        
        if (!ValidFolderNameRegex().IsMatch(input))
        {
            return Translation.Get("ProjectRenameErrorNameInvalid");
        }

        return Children?.Any(x => x.Name == input) == true ? Translation.Get("ProjectErrorNameExists") : null;
    }

    public override void HandleClick()
    {
        if (Children?.Count > 0)
        {
            IsExpanded = !IsExpanded;
        }
    }

    public void CopyAbsolutePath()
    {
        SkEditorAPI
            .Windows.GetMainWindow()?
            .Clipboard?.SetTextAsync(Path.GetFullPath(StorageFolderPath));
    }

    public void CopyPath()
    {
        Folder? root = ProjectOpener.ProjectRootFolder;
        if (root is null) return;
        
        string path = StorageFolderPath.Replace(root.StorageFolderPath, "");
        SkEditorAPI.Windows.GetMainWindow()?.Clipboard?.SetTextAsync(path);
    }

    public async Task CreateNewElement(bool file)
    {
        CreateStorageElementWindow window = new(this, file);
        await window.ShowDialog(MainWindow.Instance);
    }

    public void CreateFile(string name)
    {
        string path = Path.Combine(StorageFolderPath, name);
        System.IO.File.Create(path).Close();
        FileHandler.OpenFile(path);

        File element = new(path, this);
        Children?.Add(element);
        Sort(this);
    }

    public void CreateFolder(string name)
    {
        string path = Path.Combine(StorageFolderPath, name);
        Directory.CreateDirectory(path);

        Folder element = new(path, this);
        Children?.Add(element);
        Sort(this);
    }

    public StorageElement? GetItemByPath(string path)
    {
        if (StorageFolderPath == path)
        {
            return this;
        }
        if (Children is null)
        {
            return null;
        }

        foreach (StorageElement child in Children)
        {
            switch (child)
            {
                case Folder folder:
                {
                    StorageElement? item = folder.GetItemByPath(path);
                    if (item is not null)
                    {
                        return item;
                    }

                    break;
                }
                case File file
                    when Path.GetFullPath(file.StorageFilePath) == Path.GetFullPath(path):
                    return file;
            }
        }

        return null;
    }
    
    [System.Text.RegularExpressions.GeneratedRegex(@"^(\.)?(?!\.{1,2}$)(?!.*[\\/:*?""""<>|])(?!^[. ])(?!.*[. ]$)[\-a-zA-Z0-9][\w\-. ]{0,254}$")]
    private static partial System.Text.RegularExpressions.Regex ValidFolderNameRegex();
}