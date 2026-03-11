using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;

namespace SkEditor.Models.Projects;

public partial class LocalFolderNode : FileSystemNode
{
    private static readonly FileSystemNode DummyNode = new LocalFileNode("Loading...", "...");

    public LocalFolderNode(string name, string path) : base(name, path, true) => Children.Add(DummyNode);

    public override Symbol? Icon => Symbol.Folder;

    public override IImmutableSolidColorBrush IconColor =>
        new ImmutableSolidColorBrush(Color.Parse("#FFCF40"));

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

    public override async Task LoadChildren()
    {
        if (HasLoadedChildren)
        {
            return;
        }

        IsLoadingChildren = true;

        Children.Clear();

        List<LocalFolderNode> folders = [];
        List<LocalFileNode> files = [];

        DirectoryInfo directory = new(Path);

        foreach (FileSystemInfo entry in directory.EnumerateFileSystemInfos("*.*", SearchOption.TopDirectoryOnly))
        {
            switch (entry)
            {
                case DirectoryInfo subDirectory:
                    folders.Add(new LocalFolderNode(subDirectory.Name, subDirectory.FullName));
                    break;
                case FileInfo file:
                    files.Add(new LocalFileNode(file.Name, file.FullName));
                    break;
            }
        }

        foreach (FileSystemNode node in folders.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                     .Concat<FileSystemNode>(files.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)))
        {
            Children.Add(node);
        }

        HasLoadedChildren = true;
        IsLoadingChildren = false;
    }
}