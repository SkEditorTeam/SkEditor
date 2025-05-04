using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Files;

namespace SkEditor.Utilities.Projects.Elements;

public class File : StorageElement
{
    public File(string file, Folder? parent = null)
    {
        file = Uri.UnescapeDataString(file).NormalizePathSeparators();

        Parent = parent;
        StorageFilePath = file;

        Name = Path.GetFileName(file);
        IsFile = true;

        UpdateIcon();

        OpenInExplorerCommand = new RelayCommand(OpenInExplorer);
        DeleteCommand = new AsyncRelayCommand(DeleteFile);
        CopyAbsolutePathCommand = new RelayCommand(CopyAbsolutePath);
        CopyPathCommand = new RelayCommand(CopyPath);
    }

    public string StorageFilePath { get; set; }

    public void OpenInExplorer()
    {
        Process.Start(new ProcessStartInfo(Parent.StorageFolderPath) { UseShellExecute = true });
    }

    public async Task DeleteFile()
    {
        ContentDialogResult result = await SkEditorAPI.Windows.ShowDialog("Delete File",
            $"Are you sure you want to delete {Name} from the file system?",
            Symbol.Delete, primaryButtonText: "Delete", cancelButtonText: "Cancel", translate: false);

        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        System.IO.File.Delete(StorageFilePath);
        Parent.Children.Remove(this);
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

        StorageElement? file = Parent.Children.FirstOrDefault(x => x.Name == input);
        return file is not null ? Translation.Get("ProjectErrorNameExists") : null;
    }

    public override void RenameElement(string newName, bool move = true)
    {
        string newPath = Path.Combine(Parent.StorageFolderPath, newName);
        if (move)
        {
            System.IO.File.Move(StorageFilePath, newPath);
        }

        StorageFilePath = newPath;
        Name = newName;

        UpdateIcon();
        RefreshSelf();
    }

    public override void HandleClick()
    {
        FileHandler.OpenFile(StorageFilePath);
    }

    public void UpdateIcon()
    {
        IconSource? icon = Files.Icon.GetIcon(Path.GetExtension(StorageFilePath));
        if (icon is not null)
        {
            Icon = icon;
        }
    }

    public void CopyAbsolutePath()
    {
        SkEditorAPI.Windows.GetMainWindow().Clipboard.SetTextAsync(Path.GetFullPath(StorageFilePath));
    }

    public void CopyPath()
    {
        string path = StorageFilePath.Replace(ProjectOpener.ProjectRootFolder.StorageFolderPath, "");
        SkEditorAPI.Windows.GetMainWindow().Clipboard.SetTextAsync(path);
    }
}