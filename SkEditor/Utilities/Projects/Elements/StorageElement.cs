using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.Views;
using SkEditor.Views.Projects;
using System.Collections.ObjectModel;
using System.Linq;

namespace SkEditor.Utilities.Projects.Elements;

public abstract class StorageElement
{
    public ObservableCollection<StorageElement>? Children { get; set; }
    public Folder? Parent { get; set; }

    public string Name { get; set; } = "";
    public bool IsExpanded { get; set; } = false;

    public bool IsFile { get; set; }

    public IconSource Icon { get; set; } = new SymbolIconSource() { Symbol = Symbol.Document, FontSize = 18 };

    public RelayCommand OpenInExplorerCommand { get; set; }
    public RelayCommand RenameCommand => new(OpenRenameWindow);
    public RelayCommand DeleteCommand { get; set; }
    public RelayCommand DoubleClickCommand => new(HandleDoubleClick);
    public RelayCommand CopyPathCommand { get; set; }
    public RelayCommand CopyAbsolutePathCommand { get; set; }
    public RelayCommand CreateNewFileCommand { get; set; }
    public RelayCommand CreateNewFolderCommand { get; set; }

    public abstract string? ValidateName(string input);

    public abstract void RenameElement(string newName);

    public abstract void HandleDoubleClick();

    public async void OpenRenameWindow()
    {
        var window = new RenameElementWindow(this);
        await window.ShowDialog(MainWindow.Instance);
    }

    public void RefreshSelf()
    {
        Parent.Children[Parent.Children.IndexOf(this)] = this;
        Sort(Parent);
    }

    public static void Sort(StorageElement element)
    {
        if (element.Children == null) return;

        var temp = element.Children.ToList();
        element.Children.Clear();
        element.Children.AddRange(temp.OrderBy(x => x.IsFile).ThenBy(x => x.Name));
    }
}