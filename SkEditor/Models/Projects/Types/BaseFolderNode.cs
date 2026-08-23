using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using SkEditor.Models.Messages;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Projects.Types;

public abstract partial class BaseFolderNode : FileSystemNode
{
    private static readonly FileSystemNode DummyNode = new DummyFileNode();

    private bool _ignoreNextExpandedChange;

    public bool HasChildren => Children.Any() && Children.First() != DummyNode;

    public override IRelayCommand OpenCommand => OpenFolderCommand;

    protected abstract Func<string, Task> CreateFileAction { get; }
    protected abstract Func<string, Task> CreateFolderAction { get; }

    protected BaseFolderNode(string name, string path, bool isRoot, bool addDummy = true) : base(name, path, true)
    {
        if (!isRoot && addDummy)
        {
            Children.Add(DummyNode);
        }
    }

    [RelayCommand]
    public void OpenFolder()
    {
        IsExpanded = !IsExpanded;
    }

    public override async Task LoadChildrenAsync(bool force = false)
    {
        if (force)
        {
            HasLoadedChildren = false;
        }

        if (HasLoadedChildren)
        {
            return;
        }

        IsLoadingChildren = true;
        Children.Clear();

        try
        {
            bool folderExists = await AlreadyExistsPredicate(Path);
            if (!folderExists)
            {
                WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                    Notification
                        .Create(TranslationKey.FileExplorerLoadFailedTitle)
                        .WithMessage(TranslationKey.FileExplorerErrorDoesntExist)
                        .WithType(NotificationType.Error)));

                Log.Error("Failed to load children of folder {Path} because it does not exist", Path);
                return;
            }

            IEnumerable<FileSystemNode> nodes = await LoadFilesAndFolders();

            foreach (FileSystemNode node in nodes)
            {
                Children.Add(node);
                AddChild(node, false);
            }

            HasLoadedChildren = true;
        }
        catch (UnauthorizedAccessException ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                Notification
                    .Create(TranslationKey.FileExplorerLoadFailedTitle)
                    .WithMessage(TranslationKey.FileExplorerErrorNoPermission)
                    .WithType(NotificationType.Error)));

            Log.Error(ex, "Failed to load children of folder {Path} due to unauthorized access", Path);
            HasLoadedChildren = false;
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                Notification
                    .Create(TranslationKey.FileExplorerLoadFailedTitle)
                    .WithMessage(TranslatableString.FromRawString(ex.Message))
                    .WithType(NotificationType.Error)));

            Log.Error(ex, "Failed to load children of folder {Path}", Path);
            HasLoadedChildren = false;
        }
        finally
        {
            IsLoadingChildren = false;
        }

        _ignoreNextExpandedChange = true;
        OnPropertyChanged(nameof(IsExpanded));
    }

    public async Task NewFile(string name)
    {
        string newFilePath = CombinePath(Path, name);

        if (await AlreadyExistsPredicate(newFilePath))
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                Notification
                    .Create(TranslationKey.FileExplorerCreationFailedTitle)
                    .WithMessage(TranslationKey.ProjectErrorNameExists)
                    .WithType(NotificationType.Error)));

            return;
        }

        try
        {
            await CreateFileAction(newFilePath);

            FileSystemNode newNode = CreateFileNode(name, newFilePath);
            AddChild(newNode);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                Notification
                    .Create(TranslationKey.FileExplorerCreationFailedTitle)
                    .WithMessage(TranslatableString.FromRawString(ex.Message))
                    .WithType(NotificationType.Error)));

            Log.Error(ex, "Failed to create file {FilePath}", newFilePath);
        }
    }

    public async Task NewFolder(string name)
    {
        string newFolderPath = CombinePath(Path, name);

        if (await AlreadyExistsPredicate(newFolderPath))
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                Notification
                    .Create(TranslationKey.FileExplorerCreationFailedTitle)
                    .WithMessage(TranslationKey.ProjectErrorNameExists)
                    .WithType(NotificationType.Error)));

            return;
        }

        try
        {
            await CreateFolderAction(newFolderPath);

            FileSystemNode newNode = CreateFolderNode(name, newFolderPath);
            AddChild(newNode);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                Notification
                    .Create(TranslationKey.FileExplorerCreationFailedTitle)
                    .WithMessage(TranslatableString.FromRawString(ex.Message))
                    .WithType(NotificationType.Error)));
        }
    }

    public override async Task<bool> DeleteAsync()
    {
        try
        {
            await DeleteAction(Path);
            return true;
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                Notification
                    .Create(TranslationKey.FileExplorerDeleteFailedTitle)
                    .WithMessage(TranslatableString.FromRawString(ex.Message))
                    .WithType(NotificationType.Error)));

            Log.Error(ex, "Failed to delete file {Path}", Path);
            return false;
        }
    }

    public override async Task RenameAsync(string newName)
    {
        if (newName.IndexOfAny(InvalidChars) >= 0)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                Notification
                    .Create(TranslationKey.ProjectRenameFailedTitle)
                    .WithMessage(TranslationKey.ProjectRenameErrorNameInvalid)
                    .WithType(NotificationType.Error)));

            return;
        }

        string directory = GetDirectoryName(Path);
        string newPath = CombinePath(directory, newName);

        if (await AlreadyExistsPredicate(newPath))
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                Notification
                    .Create(TranslationKey.ProjectRenameFailedTitle)
                    .WithMessage(TranslationKey.ProjectErrorNameExists)
                    .WithType(NotificationType.Error)));

            return;
        }

        try
        {
            await RenameFileAction(Path, newPath);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                Notification
                    .Create(TranslationKey.FileExplorerRenameFailedTitle)
                    .WithMessage(TranslatableString.FromRawString(ex.Message))
                    .WithType(NotificationType.Error)));

            Log.Error(ex, "Failed to rename folder {Path} to {NewName}", Path, newName);
            return;
        }

        Name = newName;
        UpdatePath(newPath);
    }

    public override void UpdateName(string newName)
    {
        Name = newName;

        string directory = GetDirectoryName(Path);
        string newPath = CombinePath(directory, newName);
        UpdatePath(newPath);
    }

    public override void UpdatePath(string newPath)
    {
        Path = newPath;

        foreach (FileSystemNode child in Children)
        {
            child.UpdatePath(CombinePath(Path, child.Name));
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName != nameof(IsExpanded))
        {
            return;
        }

        if (_ignoreNextExpandedChange)
        {
            _ignoreNextExpandedChange = false;
            return;
        }

        if (IsExpanded && !HasLoadedChildren)
        {
            _ = LoadChildrenAsync();
        }
    }

    protected int FindInsertionIndex(FileSystemNode newNode)
    {
        bool newIsFolder = newNode is BaseFolderNode;

        int lo = 0;
        int hi = Children.Count;

        if (newIsFolder)
        {
            hi = Children.TakeWhile(c => c is BaseFolderNode).Count();
        }
        else
        {
            lo = Children.TakeWhile(c => c is BaseFolderNode).Count();
        }

        while (lo < hi)
        {
            int mid = (lo + hi) / 2;
            int cmp = StringComparer.OrdinalIgnoreCase.Compare(
                Children[mid].Name, newNode.Name);

            if (cmp < 0)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid;
            }
        }

        return lo;
    }

    public void AddChild(FileSystemNode node, bool add = true)
    {
        if (Children.Any(c => c.Path.Equals(node.Path, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }
        
        node.PropertyChanged += OnChildPropertyChanged;
        node.Deleted += OnChildDeleted;

        if (add)
        {
            Children.Insert(FindInsertionIndex(node), node);
        }
    }

    public virtual void RemoveChild(FileSystemNode node)
    {
        node.PropertyChanged -= OnChildPropertyChanged;
        node.Deleted -= OnChildDeleted;
        Children.Remove(node);
    }

    protected abstract Task<IEnumerable<FileSystemNode>> LoadFilesAndFolders();

    protected abstract FileSystemNode CreateFileNode(string name, string path);
    protected abstract FileSystemNode CreateFolderNode(string name, string path);

    private void OnChildDeleted(object? sender, EventArgs e)
    {
        if (sender is not FileSystemNode node)
        {
            return;
        }

        RemoveChild(node);
    }

    private void OnChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Name))
        {
            return;
        }

        if (sender is not FileSystemNode node)
        {
            return;
        }

        int oldIndex = Children.IndexOf(node);
        if (oldIndex < 0)
        {
            return;
        }

        int newIndex = CalculateRenameIndex(node, oldIndex);

        if (oldIndex != newIndex)
        {
            Children.Move(oldIndex, newIndex);
        }
    }

    private int CalculateRenameIndex(FileSystemNode node, int oldIndex)
    {
        int startIndex = node is BaseFolderNode ? 0 : Children.TakeWhile(c => c is BaseFolderNode).Count();
        int endIndex = node is BaseFolderNode ? Children.TakeWhile(c => c is BaseFolderNode).Count() : Children.Count;

        int targetIndex = startIndex;

        for (int i = startIndex; i < endIndex; i++)
        {
            if (i == oldIndex)
            {
                continue;
            }

            if (StringComparer.OrdinalIgnoreCase.Compare(Children[i].Name, node.Name) > 0)
            {
                break;
            }

            targetIndex++;
        }

        return targetIndex;
    }
}