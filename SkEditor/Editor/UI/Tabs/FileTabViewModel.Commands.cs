using CommunityToolkit.Mvvm.Input;
using SkEditor.Editor.Commands;

namespace SkEditor.Editor.UI.Tabs;

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
    
    [RelayCommand]
    private void TrimWhitespace()
    {
        RequestEditorCommand(EditorCommandType.TrimWhitespace);
    }

    [RelayCommand]
    private void TransformToUppercase()
    {
        RequestEditorCommand(EditorCommandType.TransformToUppercase);
    }

    [RelayCommand]
    private void TransformToLowercase()
    {
        RequestEditorCommand(EditorCommandType.TransformToLowercase);
    }

    [RelayCommand]
    private void TransformToTitlecase()
    {
        RequestEditorCommand(EditorCommandType.TransformToTitlecase);
    }

    [RelayCommand]
    private void TransformToSmallCaps()
    {
        RequestEditorCommand(EditorCommandType.TransformToSmallCaps);
    }
}