using Avalonia.Media;
using AvaloniaEdit.Editing;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.API;
using SkEditor.Views;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SkEditor.Utilities.Parser;

public partial class CodeVariable : ObservableObject, INameableCodeElement
{
    public CodeSection Section { get; private set; }

    public string Name { get; private set; }

    public bool IsLocal { get; private set; }

    public int Line { get; set; }
    public int Column { get; set; }
    public int Length { get; set; }

    public CodeVariable(CodeSection section, string raw,
        int line = -1, int column = -1)
    {
        Section = section;
        IsLocal = raw.StartsWith('_');
        Name = raw.TrimStart('_');
        Line = line;
        Column = column;
        Length = raw.Length;
    }

    public CodeVariable(CodeFunctionArgument argument)
    {
        Section = argument.Function;
        IsLocal = true;
        Name = argument.Name;
        Line = argument.Line;
        Column = argument.Column;
        Length = argument.Length;
    }

    public override string ToString() => $"{{{(IsLocal ? "_" : "")}{Name}}}";

    public bool IsSimilar(CodeVariable other) => Name == other.Name && IsLocal == other.IsLocal;

    public bool ContainsCaret(Caret caret) => caret.Line == Line && caret.Column - 1 >= Column && caret.Column - 1 <= Column + Length;

    public FontStyle Style => IsLocal ? FontStyle.Italic : FontStyle.Normal;

    public void Rename(string newName) => Rename(newName, false);

    public void Rename(string newName, bool callingFromFunction = false)
    {
        if (newName.StartsWith('{') && newName.EndsWith('}')) newName = newName[1..^1];
        if (newName.StartsWith('_')) newName = newName[1..];

        var definition = Section.GetVariableDefinition(this);
        if (definition != null && !callingFromFunction)
        {
            definition.Rename(newName);
            return;
        }

        if (IsLocal) // rename all within the section
        {
            var current = Section.Lines;
            var newLines = new List<string>();
            foreach (var line in current)
            {
                bool lineAdded = false;
                var matches = VariableRegex().Matches(line);
                foreach (var m in matches)
                {
                    var variable = new CodeVariable(Section, m.ToString(), Line, Column);
                    if (variable.IsSimilar(this))
                    {
                        var newLine = line.Replace(variable.ToString(), $"{{_{newName}}}");
                        newLines.Add(newLine);
                        lineAdded = true;
                    }
                }

                if (!lineAdded)
                    newLines.Add(line);
            }

            Section.Lines = newLines;
            Section.RefreshCode();
            Section.Parse();
        }
        else // rename all within the file
        {
            foreach (var section in Section.Parser.Sections)
            {
                var current = section.Lines;
                var newLines = new List<string>();
                foreach (var line in current)
                {
                    bool lineAdded = false;
                    var matches = VariableRegex().Matches(line);
                    foreach (var m in matches)
                    {
                        var variable = new CodeVariable(Section, m.ToString(), Line, Column);
                        if (variable.IsSimilar(this))
                        {
                            var newLine = line.Replace(variable.ToString(), $"{{{newName}}}");
                            newLines.Add(newLine);
                            lineAdded = true;
                        }
                    }

                    if (!lineAdded)
                        newLines.Add(line);
                }

                section.Lines = newLines;
                section.RefreshCode();
                section.Parse();
            }
        }
    }

    public async void Rename()
    {
        var renameWindow = new SymbolRefactorWindow(this);
        await renameWindow.ShowDialog(ApiVault.Get().GetMainWindow());
        Section.Parser.Parse();
    }

    public string GetNameDisplay()
    {
        return $"{{{(IsLocal ? "_" : "")}{Name}}}";
    }

    [GeneratedRegex(@"(?<=\{)_?(.*?)(?=\})")]
    private static partial Regex VariableRegex();
}