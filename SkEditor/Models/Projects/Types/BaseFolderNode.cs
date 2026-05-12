using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Models.Projects.Local;

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
        if (sender is not FileSystemNode node) return;
        RemoveChild(node);
    }

    private void OnChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Name)) return;
        if (sender is not FileSystemNode node) return;

        int oldIndex = Children.IndexOf(node);
        if (oldIndex < 0) return;

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
            if (i == oldIndex) continue;

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
            AddChild(node, add: false);
        }

        HasLoadedChildren = true;
        IsLoadingChildren = false;
        OnPropertyChanged(nameof(IsExpanded));
    }

    protected abstract Task<IEnumerable<FileSystemNode>> LoadFilesAndFolders();

    public abstract Task NewFile(string name);
    public abstract Task NewFolder(string name);
}