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
using SkEditor.Views;

namespace SkEditor.Utilities.Projects.Elements;

public partial class File : StorageElement
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
        if (Parent is null) return;
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
        Parent?.Children?.Remove(this);
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

        if (!ValidFileNameRegex().IsMatch(input))
        {
            return Translation.Get("ProjectRenameErrorNameInvalid");
        }

        StorageElement? file = Parent?.Children?.FirstOrDefault(x => x.Name == input);
        return file is not null ? Translation.Get("ProjectErrorNameExists") : null;
    }

    public override void RenameElement(string newName, bool move = true)
    {
        if (Parent is null) return;
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
        MainWindow? mainWindow = SkEditorAPI.Windows.GetMainWindow();
        mainWindow?.Clipboard?.SetTextAsync(Path.GetFullPath(StorageFilePath));
    }

    public void CopyPath()
    {
        Folder? root = ProjectOpener.ProjectRootFolder;
        if (root is null) return;
        
        
        string path = StorageFilePath.Replace(root.StorageFolderPath, "");
        SkEditorAPI.Windows.GetMainWindow()?.Clipboard?.SetTextAsync(path);
    }
    
    [System.Text.RegularExpressions.GeneratedRegex(@"^(\.)?(?!\.{1,2}$)(?!.*[\\/:*?""""<>|])(?!^[. ])(?!.*[. ]$)[\-a-zA-Z0-9][\w\-. ]{0,254}$")]
    private static partial System.Text.RegularExpressions.Regex ValidFileNameRegex();
}