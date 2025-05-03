using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Utilities;
using System.Linq;
using System.Text;

namespace SkEditor.Views.Generators;

public partial class CommandGenerator : AppWindow
{
    public CommandGenerator()
    {
        InitializeComponent();
        Focusable = true;

        GenerateButton.Command = new RelayCommand(Generate);
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape) Close();
        };
        Loaded += (_, _) => NameTextBox.Focus();
    }

    private void Generate()
    {
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            SkEditorAPI.Windows.ShowError(Translation.Get("CommandGeneratorPropertyMissing", "Name"));
            return;
        }

        StringBuilder code = new();

        TextEditor editor = SkEditorAPI.Files.GetCurrentOpenedFile().Editor;
        int offset = editor.CaretOffset;
        DocumentLine line = editor.Document.GetLineByOffset(offset);

        if (!string.IsNullOrEmpty(editor.Document.GetText(line.Offset, line.Length)))
        {
            editor.Document.Insert(offset, "\n\n", AnchorMovementType.AfterInsertion);
        }

        code.Append($"command /{NameTextBox.Text}:");

        AppendIfExists(ref code, "\n\tpermission: {0}", PermissionTextBox.Text);
        AppendIfExists(ref code, "\n\tpermission message: {0}", NoPermissionMessageTextBox.Text);
        AppendIfExists(ref code, "\n\taliases: {0}", AliasesTextBox.Text);
        AppendIfExists(ref code, "\n\tusage: {0}", InvalidUsageMessageTextBox.Text);
        AppendIfExists(ref code, "\n\texecutable by: {0}",
            ExecutorComboBox.SelectedItem != null
                ? ((ComboBoxItem)ExecutorComboBox.SelectedItem).Tag.ToString()
                : string.Empty);

        AppendIfExists(ref code, "\n\tcooldown: {0} {1}", CooldownQuantityTextBox.Text,
            ((ComboBoxItem)CooldownUnitComboBox.SelectedItem).Tag.ToString());

        code.Append("\n\ttrigger:\n\t\t");

        editor.Document.Insert(editor.CaretOffset, code.ToString(), AnchorMovementType.AfterInsertion);
        Close();
    }

    private static void AppendIfExists(ref StringBuilder code, string template, params string[] values)
    {
        if (values.Any(string.IsNullOrWhiteSpace)) return;
        for (int i = 0; i < values.Length; i++)
        {
            template = template.Replace($"{{{i}}}", values[i]);
        }

        code.Append(template);
    }
}