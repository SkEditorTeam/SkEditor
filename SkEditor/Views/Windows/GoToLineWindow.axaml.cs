using System;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;

namespace SkEditor.Views.Windows;

public partial class GoToLineWindow : AppWindow
{
    public GoToLineWindow()
    {
        InitializeComponent();
        GoToLineInput.Loaded += (_, _) => GoToLineInput.Focus();
        GoToLineInput.TextChanged += (_, _) => UpdateInput();
        GoToLineButton.Command = new RelayCommand(Execute);

        KeyDown += (_, e) =>
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Execute();
                    break;
                case Key.Escape:
                    Close();
                    break;
            }
        };
    }

    private void Execute()
    {
        if (string.IsNullOrWhiteSpace(GoToLineInput.Text))
        {
            Close();
            return;
        }

        if (!SkEditorAPI.Files.IsEditorOpen())
        {
            return;
        }

        TextEditor? editor = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor;
        if (editor == null) return;
        if (!int.TryParse((string?)GoToLineInput.Text, out int lineNumber))
        {
            return;
        }

        DocumentLine line = editor.Document.GetLineByNumber(lineNumber);
        editor.ScrollTo(line.LineNumber, 0);
        editor.Focus();
        editor.CaretOffset = line.Offset;
        Close();
    }

    private void UpdateInput()
    {
        TextEditor? editor = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor;
        if (editor == null) return;
        int documentLines = editor.Document.LineCount;
        if (!int.TryParse((string?)GoToLineInput.Text, out int line))
        {
            GoToLineInput.Text = "";
            return;
        }

        line = Math.Clamp(line, 1, documentLines);
        GoToLineInput.Text = line.ToString();
    }
}