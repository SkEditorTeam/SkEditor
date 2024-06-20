using System;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;

namespace SkEditor.Utilities.Editor;
public partial class GoToLine : AppWindow
{

    public GoToLine()
    {
        InitializeComponent();
        GoToLineInput.Loaded += (sender, e) => GoToLineInput.Focus();
        GoToLineInput.TextChanged += (_, _) => UpdateInput();
        GoToLineButton.Command = new RelayCommand(Execute);
        
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter) Execute();
            if (e.Key == Key.Escape) Close();
        };
    }

    private void Execute()
    {
        if (string.IsNullOrWhiteSpace(GoToLineInput.Text))
        {
            Close();
            return;
        }

        TextEditor editor = ApiVault.Get().GetTextEditor();
        if (!int.TryParse(GoToLineInput.Text, out int lineNumber)) return;
        DocumentLine line = editor.Document.GetLineByNumber(lineNumber);
        editor.ScrollTo(line.LineNumber, 0);
        editor.Focus();
        editor.CaretOffset = line.Offset;
        Close();
    }

    private void UpdateInput()
    {
        TextEditor editor = ApiVault.Get().GetTextEditor();
        int documentLines = editor.Document.LineCount;
        if (!int.TryParse(GoToLineInput.Text, out int line)) {
            GoToLineInput.Text = "";
            return;
        };
        line = Math.Clamp(line, 1, documentLines);
        GoToLineInput.Text = line.ToString();
    }
}
