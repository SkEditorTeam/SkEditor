using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Files;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SkEditor.Utilities.Projects.Elements;

public class File : StorageElement
{
    public string StorageFilePath { get; set; }

    public File(string file, Folder? parent = null)
    {
        file = Uri.UnescapeDataString(file).FixLinuxPath();

        Parent = parent;
        StorageFilePath = file;

        Name = Path.GetFileName(file);
        IsFile = true;

        var icon = Files.Icon.GetIcon(Path.GetExtension(file));
        if (icon is not null) Icon = icon;

        OpenInExplorerCommand = new RelayCommand(OpenInExplorer);
        DeleteCommand = new RelayCommand(DeleteFile);
        CopyAbsolutePathCommand = new RelayCommand(CopyAbsolutePath);
        CopyPathCommand = new RelayCommand(CopyPath);
    }

    public void OpenInExplorer()
    {
        Process.Start(new ProcessStartInfo(Parent.StorageFolderPath) { UseShellExecute = true });
    }

    public async void DeleteFile()
    {
        var result = await SkEditorAPI.Windows.ShowDialog("Delete File", 
            $"Are you sure you want to delete {Name} from the file system?",
            icon: Symbol.Delete, primaryButtonText: "Delete", cancelButtonText: "Cancel", translate: false);

        if (result != ContentDialogResult.Primary) return;

        System.IO.File.Delete(StorageFilePath);
        Parent.Children.Remove(this);
    }

    public override string? ValidateName(string input)
    {
        if (input == Name) return Translation.Get("ProjectRenameErrorSameName");
        if (Parent is null) return Translation.Get("ProjectRenameErrorParentNull");

        var file = Parent.Children.FirstOrDefault(x => x.Name == input);
        if (file is not null) return Translation.Get("ProjectErrorNameExists");

        return null;
    }

    public override void RenameElement(string newName, bool move = true)
    {
        var newPath = Path.Combine(Parent.StorageFolderPath, newName);
        if (move) System.IO.File.Move(StorageFilePath, newPath);

        StorageFilePath = newPath;
        Name = newName;

        RefreshSelf();
    }

    public override void HandleClick() => FileHandler.OpenFile(StorageFilePath);

    public void CopyAbsolutePath()
    {
        SkEditorAPI.Windows.GetMainWindow().Clipboard.SetTextAsync(Path.GetFullPath(StorageFilePath));
    }

    public void CopyPath()
    {
        var path = StorageFilePath.Replace(ProjectOpener.ProjectRootFolder.StorageFolderPath, "");
        SkEditorAPI.Windows.GetMainWindow().Clipboard.SetTextAsync(path);
    }
}