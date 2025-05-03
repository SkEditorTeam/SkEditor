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
using SkEditor.Views;
using SkEditor.Views.Projects;
using static SkEditor.Controls.Sidebar.ExplorerSidebarPanel;

namespace SkEditor.Utilities.Projects.Elements;

public class Folder : StorageElement
{
    public string StorageFolderPath { get; set; }

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
            var segments = folder.Split(
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

    private async Task LoadChildren()
    {
        await Task.Run(
            () =>
                Dispatcher.UIThread.Invoke(() =>
                {
                    Directory
                        .GetDirectories(StorageFolderPath)
                        .ToList()
                        .ForEach(x => Children.Add(new Folder(x, this)));

                    Directory
                        .GetFiles(StorageFolderPath)
                        .ToList()
                        .ForEach(x => Children.Add(new File(x, this)));
                })
        );
    }

    public void OpenInExplorer()
    {
        Process.Start(new ProcessStartInfo(StorageFolderPath) { UseShellExecute = true });
    }

    public async Task DeleteFolder()
    {
        var result = await SkEditorAPI.Windows.ShowDialog(
            "Delete File",
            $"Are you sure you want to delete {Name} from the file system?",
            icon: Symbol.Delete,
            primaryButtonText: "Delete",
            cancelButtonText: "Cancel",
            translate: false
        );

        if (result != ContentDialogResult.Primary)
            return;

        Directory.Delete(StorageFolderPath, true);
        Parent?.Children?.Remove(this);

        if (Parent is null)
            CloseProject();
    }

    private static void CloseProject()
    {
        ProjectOpener.FileTreeView.ItemsSource = null;

        Folder? projectRootFolder = null;

        ExplorerPanel? panel =
            Registries.SidebarPanels.FirstOrDefault(x => x is ExplorerPanel) as ExplorerPanel;

        StackPanel noFolderMessage = panel.Panel.NoFolderMessage;
        noFolderMessage.IsVisible = projectRootFolder == null;
    }

    public override void RenameElement(string newName, bool move = true)
    {
        var newPath = Path.Combine(Parent.StorageFolderPath, newName);

        if (move)
            Directory.Move(StorageFolderPath, newPath);

        StorageFolderPath = newPath;
        Name = newName;
        RefreshSelf();
    }

    public override string? ValidateName(string input)
    {
        if (input == Name)
            return Translation.Get("ProjectRenameErrorSameName");

        if (Parent is null)
            return Translation.Get("ProjectRenameErrorParentNull");

        var folder = Parent.Children.FirstOrDefault(x => x.Name == input);

        return folder is not null ? Translation.Get("ProjectErrorNameExists") : null;
    }

    public string? ValidateCreationName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Translation.Get("ProjectCreateErrorNameEmpty");

        if (Children.Any(x => x.Name == input))
            return Translation.Get("ProjectErrorNameExists");

        return null;
    }

    public override void HandleClick()
    {
        if (Children.Count > 0)
            IsExpanded = !IsExpanded;
    }

    public void CopyAbsolutePath()
    {
        SkEditorAPI
            .Windows.GetMainWindow()
            .Clipboard.SetTextAsync(Path.GetFullPath(StorageFolderPath));
    }

    public void CopyPath()
    {
        var path = StorageFolderPath.Replace(ProjectOpener.ProjectRootFolder.StorageFolderPath, "");
        SkEditorAPI.Windows.GetMainWindow().Clipboard.SetTextAsync(path);
    }

    public async Task CreateNewElement(bool file)
    {
        var window = new CreateStorageElementWindow(this, file);
        await window.ShowDialog(MainWindow.Instance);
    }

    public void CreateFile(string name)
    {
        var path = Path.Combine(StorageFolderPath, name);
        System.IO.File.Create(path).Close();
        FileHandler.OpenFile(path);

        var element = new File(path, this);
        Children.Add(element);
        Sort(this);
    }

    public void CreateFolder(string name)
    {
        var path = Path.Combine(StorageFolderPath, name);
        Directory.CreateDirectory(path);

        var element = new Folder(path, this);
        Children.Add(element);
        Sort(this);
    }

    public StorageElement? GetItemByPath(string path)
    {
        if (StorageFolderPath == path)
            return this;

        foreach (var child in Children)
        {
            switch (child)
            {
                case Folder folder:
                {
                    var item = folder.GetItemByPath(path);
                    if (item is not null)
                        return item;
                    break;
                }
                case File file
                    when Path.GetFullPath(file.StorageFilePath) == Path.GetFullPath(path):
                    return file;
            }
        }

        return null;
    }
}