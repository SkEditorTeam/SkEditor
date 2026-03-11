using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;

namespace SkEditor.Models.Projects;

public abstract partial class BaseFolderNode : FileSystemNode
{
    private static readonly FileSystemNode DummyNode = new LocalFileNode("Loading...", "...");
    private readonly bool _isRoot;

    protected BaseFolderNode(string name, string path, bool isRoot) : base(name, path, true)
    {
        _isRoot = isRoot;
        if (!isRoot)
        {
            Children.Add(DummyNode);
        }
    }

    public override Symbol? Icon => _isRoot ? Symbol.Desktop : IsExpanded ? Symbol.FolderOpen : Symbol.Folder;

    public override IRelayCommand OpenCommand => OpenFolderCommand;

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

        OnPropertyChanged(nameof(Icon));
    }
}