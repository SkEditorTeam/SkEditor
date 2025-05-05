using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AvaloniaEdit.Editing;
using SkEditor.API;
using SkEditor.Views;

namespace SkEditor.Utilities.Parser;

public class CodeOption : INameableCodeElement
{
    public CodeOption(CodeSection section, string line,
        int lineIndex = -1, int column = -1)
    {
        Section = section;
        // don't forget to remove the starting tab/spaces
        Name = line.TrimStart(' ', '\t').Split(':')[0];
        Line = lineIndex;
        Column = column;
        Length = Name.Length;
    }

    public CodeSection Section { get; }

    public int Line { get; set; }
    public int Column { get; set; }
    public int Length { get; set; }
    public string Name { get; }

    public void Rename(string newName)
    {
        // First rename the option declaration
        string currentLine = Section.Lines[Line - Section.StartingLineIndex - 1];
        Regex regex = new(Regex.Escape(Name));
        string newCurrentLine = regex.Replace(currentLine, newName, 1);
        Section.Lines[Line - Section.StartingLineIndex - 1] = newCurrentLine;
        Length = newName.Length;
        Section.RefreshCode();

        // Then rename all references
        foreach (CodeSection section in Section.Parser.Sections)
        {
            foreach (CodeOptionReference reference in section.OptionReferences)
            {
                if (reference.IsSimilar(this))
                {
                    reference.Replace(newName);
                }
            }
        }
    }

    public bool ContainsCaret(Caret caret)
    {
        return caret.Line == Line && caret.Column - 1 >= Column && caret.Column - 1 <= Column + Length;
    }

    public async Task Rename()
    {
        SymbolRefactorWindow renameWindow = new(this);
        await renameWindow.ShowDialog(SkEditorAPI.Windows.GetMainWindow());
        Section.Parser.Parse();
    }
}