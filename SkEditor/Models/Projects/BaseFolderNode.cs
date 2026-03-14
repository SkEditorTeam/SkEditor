using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Models.Projects.Local;

namespace SkEditor.Models.Projects;

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
        }

        HasLoadedChildren = true;
        IsLoadingChildren = false;
        OnPropertyChanged(nameof(IsExpanded));
    }

    protected abstract Task<IEnumerable<FileSystemNode>> LoadFilesAndFolders();
}