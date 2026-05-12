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
            _ = LoadChildren();
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

    protected void AddChild(FileSystemNode node)
    {
        node.PropertyChanged += OnChildPropertyChanged;
        Children.Insert(FindInsertionIndex(node), node);
    }

    protected void RemoveChild(FileSystemNode node)
    {
        node.PropertyChanged -= OnChildPropertyChanged;
        Children.Remove(node);
    }

    private void OnChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Name))
        {
            return;
        }

        if (sender is not FileSystemNode node) return;

        int oldIndex = Children.IndexOf(node);
        if (oldIndex < 0) return;

        int newIndex = FindInsertionIndex(node);

        if (oldIndex != newIndex)
        {
            Children.Move(oldIndex, newIndex);
        }
    }

    public override async Task LoadChildren()
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
            node.PropertyChanged += OnChildPropertyChanged;
        }

        HasLoadedChildren = true;
        IsLoadingChildren = false;
        OnPropertyChanged(nameof(IsExpanded));
    }

    protected abstract Task<IEnumerable<FileSystemNode>> LoadFilesAndFolders();

    public abstract Task NewFile(string name);
    public abstract Task NewFolder(string name);
}