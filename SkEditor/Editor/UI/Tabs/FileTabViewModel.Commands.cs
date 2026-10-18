using CommunityToolkit.Mvvm.Input;
using SkEditor.Utilities.Editor;

namespace SkEditor.ViewModels.Tabs;

public partial class FileTabViewModel
{
    [RelayCommand]
    private void Copy()
    {
        RequestEditorCommand(EditorCommandType.Copy);
    }

    [RelayCommand]
    private void Cut()
    {
        RequestEditorCommand(EditorCommandType.Cut);
    }

    [RelayCommand]
    private void Paste()
    {
        RequestEditorCommand(EditorCommandType.Paste);
    }

    [RelayCommand]
    private void Undo()
    {
        RequestEditorCommand(EditorCommandType.Undo);
    }

    [RelayCommand]
    private void Redo()
    {
        RequestEditorCommand(EditorCommandType.Redo);
    }

    [RelayCommand]
    private void Comment()
    {
        RequestEditorCommand(EditorCommandType.Comment);
    }

    [RelayCommand]
    private void Find()
    {
        RequestEditorCommand(EditorCommandType.Find);
    }

    [RelayCommand]
    private void Replace()
    {
        RequestEditorCommand(EditorCommandType.Replace);
    }

    [RelayCommand]
    private void SelectAll()
    {
        RequestEditorCommand(EditorCommandType.SelectAll);
    }
}