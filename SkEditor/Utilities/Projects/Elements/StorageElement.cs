using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Views;
using SkEditor.Views.Projects;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SkEditor.Utilities.Projects.Elements;

public abstract class StorageElement
{
    public ObservableCollection<StorageElement>? Children { get; set; }
    protected Folder? Parent { get; set; }

    public string Name { get; set; } = "";
    public bool IsExpanded { get; set; }

    public bool IsFile { get; set; }
    public bool IsRootFolder { get; set; }

    public IconSource Icon { get; set; } = new SymbolIconSource() { Symbol = Symbol.Document, FontSize = 18 };

    public IRelayCommand OpenInExplorerCommand { get; set; }
    public IRelayCommand RenameCommand => new AsyncRelayCommand(OpenRenameWindow);
    public IRelayCommand DeleteCommand { get; set; }
    public IRelayCommand DoubleClickCommand => new RelayCommand(HandleDoubleClick);
    public IRelayCommand SingleClickCommand => new RelayCommand(HandleSingleClick);
    public IRelayCommand CopyPathCommand { get; set; }
    public IRelayCommand CopyAbsolutePathCommand { get; set; }
    public IRelayCommand CreateNewFileCommand { get; set; }
    public IRelayCommand CreateNewFolderCommand { get; set; }
    public IRelayCommand CloseProjectCommand { get; set; }

    public abstract string? ValidateName(string input);

    public abstract void RenameElement(string newName, bool move = true);

    public abstract void HandleClick();

    public void HandleDoubleClick()
    {
        if (!SkEditorAPI.Core.GetAppConfig().IsProjectSingleClickEnabled)
            HandleClick();
    }
    public void HandleSingleClick()
    {
        if (SkEditorAPI.Core.GetAppConfig().IsProjectSingleClickEnabled)
            HandleClick();
    }

    public async Task OpenRenameWindow()
    {
        var window = new RenameElementWindow(this);
        await window.ShowDialog(MainWindow.Instance);
    }

    protected void RefreshSelf()
    {
        Parent.Children[Parent.Children.IndexOf(this)] = this;
        Sort(Parent);
    }

    protected static void Sort(StorageElement element)
    {
        if (element.Children == null) return;

        var temp = element.Children.ToList();
        element.Children.Clear();
        element.Children.AddRange(temp.OrderBy(x => x.IsFile).ThenBy(x => x.Name));
    }
}