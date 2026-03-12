using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;

namespace SkEditor.Models.Projects;

public abstract partial class FileSystemNode(string name, string path, bool isDirectory) : ObservableObject
{
    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isLoadingChildren;

    [ObservableProperty]
    private bool _isRenaming;

    [ObservableProperty]
    private string _name = name;

    protected string Path { get; set; } = path;
    public bool IsDirectory { get; set; } = isDirectory;

    public ObservableCollection<FileSystemNode> Children { get; } = [];

    public virtual Symbol? Icon => null;
    public virtual IImmutableSolidColorBrush IconColor => Brushes.White;
    public virtual IImage? IconImage => null;

    public virtual IRelayCommand? OpenCommand => null;

    protected bool HasLoadedChildren { get; set; }

    public abstract Task LoadChildren();
}