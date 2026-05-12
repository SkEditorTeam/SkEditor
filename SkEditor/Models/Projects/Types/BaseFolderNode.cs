using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using SkEditor.Models.Messages;
using SkEditor.Models.Projects.Local;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Projects.Types;

public abstract partial class BaseFolderNode : FileSystemNode
{
    private static readonly FileSystemNode DummyNode = new LocalFileNode("Loading...", "...");

    protected BaseFolderNode(string name, string path, bool isRoot) : base(name, path, true)
    {
        if (!isRoot)
        {
            Children.Add(DummyNode);
        }
    }

    public override IRelayCommand OpenCommand => OpenFolderCommand;

    public bool HasChildren => Children.Any() && Children.First() != DummyNode;

    protected abstract Func<string, Task<bool>> AlreadyExistsPredicate { get; }
    protected abstract Func<string, Task> CreateFileAction { get; }
    protected abstract Func<string, Task> CreateFolderAction { get; }

    [RelayCommand]
    public void OpenFolder()
    {
        IsExpanded = !IsExpanded;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName != nameof(IsExpanded))
        {
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

    protected void AddChild(FileSystemNode node, bool add = true)
    {
        node.PropertyChanged += OnChildPropertyChanged;
        node.Deleted += OnChildDeleted;

        if (add)
        {
            Children.Insert(FindInsertionIndex(node), node);
        }
    }

    protected void RemoveChild(FileSystemNode node)
    {
        node.PropertyChanged -= OnChildPropertyChanged;
        node.Deleted -= OnChildDeleted;
        Children.Remove(node);
    }

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

    public override async Task LoadChildrenAsync()
    {
        if (HasLoadedChildren)
        {
            return;
        }

        IsLoadingChildren = true;
        Children.Clear();

        IEnumerable<FileSystemNode> nodes = await LoadFilesAndFolders();

        foreach (FileSystemNode node in nodes)
        {
            Children.Add(node);
            AddChild(node, false);
        }

        HasLoadedChildren = true;
        IsLoadingChildren = false;
        OnPropertyChanged(nameof(IsExpanded));
    }

    protected abstract Task<IEnumerable<FileSystemNode>> LoadFilesAndFolders();

    public async Task NewFile(string name)
    {
        string newFilePath = System.IO.Path.Combine(Path, name);

        if (await AlreadyExistsPredicate(newFilePath))
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslatableString.FromRawString("File Creation Failed"),
                TranslationKey.ProjectErrorNameExists, NotificationType.Error));

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
                TranslatableString.FromRawString("File Creation Failed"),
                TranslatableString.FromRawString(ex.Message), NotificationType.Error));

            Log.Error(ex, "Failed to create file {FilePath}", newFilePath);
        }
    }

    public async Task NewFolder(string name)
    {
        string newFolderPath = System.IO.Path.Combine(Path, name);

        if (await AlreadyExistsPredicate(newFolderPath))
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslatableString.FromRawString("Folder Creation Failed"),
                TranslationKey.ProjectErrorNameExists, NotificationType.Error));

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
                TranslatableString.FromRawString("Folder Creation Failed"),
                TranslatableString.FromRawString(ex.Message), NotificationType.Error));
        }
    }

    protected abstract FileSystemNode CreateFileNode(string name, string path);
    protected abstract FileSystemNode CreateFolderNode(string name, string path);
}