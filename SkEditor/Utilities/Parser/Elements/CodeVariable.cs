using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Media;
using AvaloniaEdit.Editing;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Extensions;
using SymbolRefactorWindow = SkEditor.Views.Windows.SymbolRefactorWindow;

namespace SkEditor.Utilities.Parser;

public partial class CodeVariable : ObservableObject, INameableCodeElement
{
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

    public CodeSection Section { get; }

    public bool IsLocal { get; }

    public int Line { get; set; }
    public int Column { get; set; }
    public int Length { get; set; }

    public FontStyle Style => IsLocal ? FontStyle.Italic : FontStyle.Normal;

    public string Name { get; }

    public void Rename(string newName)
    {
        Rename(newName, false);
    }

    public string GetNameDisplay()
    {
        return $"{{{(IsLocal ? "_" : "")}{Name}}}";
    }

    public override string ToString()
    {
        return $"{{{(IsLocal ? "_" : "")}{Name}}}";
    }

    public bool IsSimilar(CodeVariable other)
    {
        return Name == other.Name && IsLocal == other.IsLocal;
    }

    public bool ContainsCaret(Caret caret)
    {
        return caret.Line == Line && caret.Column - 1 >= Column && caret.Column - 1 <= Column + Length;
    }

    public void Rename(string newName, bool callingFromFunction)
    {
        if (newName.StartsWith('{') && newName.EndsWith('}'))
        {
            newName = newName[1..^1];
        }

        if (newName.StartsWith('_'))
        {
            newName = newName[1..];
        }

        CodeFunctionArgument? definition = Section.GetVariableDefinition(this);
        if (definition != null && !callingFromFunction)
        {
            definition.Rename(newName);
            return;
        }

        if (IsLocal) // rename all within the section
        {
            List<string> current = Section.Lines;
            List<string> newLines = new();
            foreach (string line in current)
            {
                bool lineAdded = false;
                MatchCollection matches = VariableRegex().Matches(line);
                foreach (Match m in matches)
                {
                    CodeVariable variable = new(Section, m.ToString(), Line, Column);
                    if (!variable.IsSimilar(this))
                    {
                        continue;
                    }

                    string newLine = line.Replace(variable.ToString(), $"{{_{newName}}}");
                    newLines.Add(newLine);
                    lineAdded = true;
                }

                if (!lineAdded)
                {
                    newLines.Add(line);
                }
            }

            Section.Lines = newLines;
            Section.RefreshCode();
            Section.Parse();
        }
        else // rename all within the file
        {
            foreach (CodeSection section in Section.Parser.Sections)
            {
                List<string> current = section.Lines;
                List<string> newLines = [];
                foreach (string line in current)
                {
                    bool lineAdded = false;
                    MatchCollection matches = VariableRegex().Matches(line);
                    foreach (Match m in matches)
                    {
                        CodeVariable variable = new(Section, m.ToString(), Line, Column);
                        if (!variable.IsSimilar(this))
                        {
                            continue;
                        }

                        string newLine = line.Replace(variable.ToString(), $"{{{newName}}}");
                        newLines.Add(newLine);
                        lineAdded = true;
                    }

                    if (!lineAdded)
                    {
                        newLines.Add(line);
                    }
                }

                section.Lines = newLines;
                section.RefreshCode();
                section.Parse();
            }
        }
    }

    public async Task Rename()
    {
        SymbolRefactorWindow renameWindow = new(this);
        await renameWindow.ShowDialogOnMainWindow();
        Section.Parser.Parse();
    }

    [GeneratedRegex(@"(?<=\{)_?(.*?)(?=\})")]
    private static partial Regex VariableRegex();
}