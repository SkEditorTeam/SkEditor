using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.Views;
using SkEditor.Views.Projects;

namespace SkEditor.Utilities.Projects.Elements;

public class Folder : StorageElement
{
    
    public string StorageFolderPath { get; set; }

    public Folder(string folder, Folder? parent = null)
    {
        Parent = parent;
        StorageFolderPath = folder;
        Name = Path.GetFileName(folder);
        IsFile = false;
        
        Children = new ObservableCollection<StorageElement>();
        LoadChildren();
        
        // Commands
        OpenInExplorerCommand = new RelayCommand(OpenInExplorer);
        DeleteCommand = new RelayCommand(DeleteFolder);
        CopyPathCommand = new RelayCommand(CopyPath);
        CopyAbsolutePathCommand = new RelayCommand(CopyAbsolutePath);
        CreateNewFileCommand = new RelayCommand(() => CreateNewElement(true));
        CreateNewFolderCommand = new RelayCommand(() => CreateNewElement(false));
    }

    private void LoadChildren()
    {
        foreach (var child in Directory.GetDirectories(StorageFolderPath))
            Children.Add(new Folder(child, this));
        
        foreach (var child in Directory.GetFiles(StorageFolderPath))
            Children.Add(new File(child, this));
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

    public override void RenameElement(string newName)
    {
        var newPath = Path.Combine(Parent.StorageFolderPath, newName);
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
        if (folder is not null)
            return Translation.Get("ProjectRenameErrorNameExists");
        
        return null;
    }

    public override void HandleDoubleClick()
    {
        if (Children.Count == 0)
            return;
        
        IsExpanded = !IsExpanded;
    }
    
    public void CopyAbsolutePath()
    {
        ApiVault.Get().GetMainWindow().Clipboard.SetTextAsync(StorageFolderPath.Replace("\\", "/"));
    }
    
    public void CopyPath()
    {
        var path = StorageFolderPath.Replace(ProjectOpener.ProjectRootFolder.StorageFolderPath, "");
        ApiVault.Get().GetMainWindow().Clipboard.SetTextAsync(path.Replace("\\", "/"));
    }

    public async void CreateNewElement(bool file)
    {
        var window = new CreateStorageElementWindow(this, file);
        await window.ShowDialog(MainWindow.Instance);
    }
}