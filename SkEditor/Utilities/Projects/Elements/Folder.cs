using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.Utilities.Files;
using SkEditor.Views;
using SkEditor.Views.Projects;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SkEditor.Utilities.Projects.Elements;

public class Folder : StorageElement
{
    public string StorageFolderPath { get; set; }

    public Folder(string folder, Folder? parent = null)
    {
        folder = Uri.UnescapeDataString(folder).Replace("/", "\\");

        Parent = parent;
        StorageFolderPath = folder;
        Name = Path.GetFileName(folder);
        IsFile = false;

        Children = [];
        LoadChildren();

        OpenInExplorerCommand = new RelayCommand(OpenInExplorer);
        DeleteCommand = new RelayCommand(DeleteFolder);
        CopyPathCommand = new RelayCommand(CopyPath);
        CopyAbsolutePathCommand = new RelayCommand(CopyAbsolutePath);
        CreateNewFileCommand = new RelayCommand(() => CreateNewElement(true));
        CreateNewFolderCommand = new RelayCommand(() => CreateNewElement(false));
    }

    private void LoadChildren()
    {
        try
        {
            Directory.GetDirectories(StorageFolderPath).ToList().ForEach(x => Children.Add(new Folder(x, this)));
            Directory.GetFiles(StorageFolderPath).ToList().ForEach(x => Children.Add(new File(x, this)));
        }
        catch (Exception e)
        {
            // TODO: Correctly handle this error (usually access denied)
        }
    }

    public void OpenInExplorer()
    {
        Process.Start(new ProcessStartInfo(StorageFolderPath) { UseShellExecute = true });
    }

    public void DeleteFolder()
    {
        Directory.Delete(StorageFolderPath, true);
        Parent.Children.Remove(this);
    }

    public override void RenameElement(string newName, bool move = true)
    {
        var newPath = Path.Combine(Parent.StorageFolderPath, newName);
        if (move) Directory.Move(StorageFolderPath, newPath);

        StorageFolderPath = newPath;
        Name = newName;

        RefreshSelf();
    }

    public override string? ValidateName(string input)
    {
        if (input == Name) return Translation.Get("ProjectRenameErrorSameName");
        if (Parent is null) return Translation.Get("ProjectRenameErrorParentNull");

        var folder = Parent.Children.FirstOrDefault(x => x.Name == input);
        if (folder is not null) return Translation.Get("ProjectErrorNameExists");

        return null;
    }

    public string? ValidateCreationName(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return Translation.Get("ProjectCreateErrorNameEmpty");

        if (Children.Any(x => x.Name == input)) return Translation.Get("ProjectErrorNameExists");

        return null;
    }

    public override void HandleDoubleClick()
    {
        if (Children.Count > 0) IsExpanded = !IsExpanded;
    }

    public void CopyAbsolutePath()
    {
        SkEditorAPI.Windows.GetMainWindow().Clipboard.SetTextAsync(Path.GetFullPath(StorageFolderPath));
    }

    public void CopyPath()
    {
        var path = StorageFolderPath.Replace(ProjectOpener.ProjectRootFolder.StorageFolderPath, "");
        SkEditorAPI.Windows.GetMainWindow().Clipboard.SetTextAsync(path);
    }

    public async void CreateNewElement(bool file)
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
        if (StorageFolderPath == path) return this;

        foreach (var child in Children)
        {
            if (child is Folder folder)
            {
                var item = folder.GetItemByPath(path);
                if (item is not null) return item;
            }
            else if (child is File file && Path.GetFullPath(file.StorageFilePath) == Path.GetFullPath(path))
            {
                return file;
            }
        }

        return null;
    }
}