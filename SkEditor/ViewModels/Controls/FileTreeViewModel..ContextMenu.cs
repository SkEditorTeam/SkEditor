using CommunityToolkit.Mvvm.Input;
using SkEditor.Models.Projects;
using SkEditor.Utilities;

namespace SkEditor.ViewModels.Controls;

public partial class FileTreeViewModel
{
    [RelayCommand]
    public void OpenInExplorer(FileSystemNode node)
    {
        if (node.IsDirectory)
        {
            LinkOpener.OpenFolder(node.Path);
        }
        else
        {
            LinkOpener.OpenFolderWithFileSelection(node.Path);
        }
    }

    [RelayCommand]
    public void NewFile(FileSystemNode node)
    {
        if (!node.IsDirectory)
        {
            return;
        }

        notifs.Debug("New file creation is not implemented yet.");
    }

    [RelayCommand]
    public void NewFolder(FileSystemNode node)
    {
        if (!node.IsDirectory)
        {
            return;
        }

        notifs.Debug("New folder creation is not implemented yet.");
    }

    [RelayCommand]
    public void Rename(FileSystemNode node)
    {
        notifs.Debug("Renaming is not implemented yet.");
    }

    [RelayCommand]
    public void Delete(FileSystemNode node)
    {
        notifs.Debug("Delete is not implemented yet.");
    }

    [RelayCommand]
    public void CopyPath(FileSystemNode node)
    {
        notifs.Debug("CopyPath is not implemented yet.");
    }

    [RelayCommand]
    public void CopyRelativePath(FileSystemNode node)
    {
        notifs.Debug("CopyRelativePath is not implemented yet.");
    }

    [RelayCommand]
    public void Download(FileSystemNode node)
    {
        notifs.Debug("Download is not implemented yet.");
    }

    [RelayCommand]
    public void Refresh(FileSystemNode node)
    {
        notifs.Debug("Refresh is not implemented yet.");
    }

    [RelayCommand]
    public void EditConnection(FileSystemNode node)
    {
        notifs.Debug("EditConnection is not implemented yet.");
    }

    [RelayCommand]
    public void CloseWorkspace(FileSystemNode node)
    {
        notifs.Debug("CloseWorkspace is not implemented yet.");
    }

    [RelayCommand]
    public void Disconnect(FileSystemNode node)
    {
        notifs.Debug("Disconnect is not implemented yet.");
    }
}