using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace SkEditor.Models.Projects;

public partial class SftpFolderNode : FileSystemNode
{
    private static readonly FileSystemNode DummyNode = new LocalFileNode("Loading...", "...");
    private readonly SftpClient _client;
    private readonly bool _isRoot;

    public SftpFolderNode(string name, string path, bool isRoot, SftpClient client) : base(name, path, true)
    {
        _isRoot = isRoot;
        _client = client;

        if (isRoot)
        {
            return;
        }

        Children.Add(DummyNode);
    }

    public override Symbol? Icon => _isRoot ? Symbol.Desktop : IsExpanded ? Symbol.FolderOpen : Symbol.Folder;

    public override IImmutableSolidColorBrush IconColor =>
        new ImmutableSolidColorBrush(Color.Parse(_isRoot ? "#8CA9FF" : "#FFCF40"));

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

        List<SftpFolderNode> folders = [];
        List<SftpFileNode> files = [];

        await foreach (ISftpFile file in _client.ListDirectoryAsync(Path, CancellationToken.None))
        {
            if (file.IsDirectory)
            {
                if (file.Name is "." or "..")
                    continue;

                folders.Add(new SftpFolderNode(file.Name, file.FullName, false, _client));
            }
            else if (file.IsRegularFile)
            {
                files.Add(new SftpFileNode(file.Name, file.FullName, _client));
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