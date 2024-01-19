using System.Text.RegularExpressions;
using AvaloniaEdit.Editing;
using SkEditor.API;

namespace SkEditor.Utilities.Parser;

public class CodeOption : INameableCodeElement
{
    public string Name { get; }
    public CodeSection Section { get; }
    
    public int Line { get; set; }
    public int Column { get; set; }
    public int Length { get; set; }

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
    
    public bool ContainsCaret(Caret caret)
    {
        return caret.Line == Line && caret.Column - 1 >= Column && caret.Column - 1 <= Column + Length;
    }
    
    public void Rename(string newName)
    {
        // First rename the option declaration
        var line = Section.Lines[Line - Section.StartingLineIndex - 1];
        var regex = new Regex(Regex.Escape(Name));
        var newLine = regex.Replace(line, newName, 1);
        Section.Lines[Line - Section.StartingLineIndex -1] = newLine;
        Length = newName.Length;
        Section.RefreshCode();
        
        // Then rename the option usage everywhere (form is '{@optionName}')
        var regex2 = new Regex($"{{@{Regex.Escape(Name)}}}");
        foreach (var section in Section.Parser.Sections)
        {
            for (var index = section.StartingLineIndex; index < section.StartingLineIndex + section.Lines.Count; index++)
            {
                var line2 = section.Lines[index - section.StartingLineIndex];
                var newLine2 = regex2.Replace(line2, $"{{@{newName}}}");
                section.Lines[index - section.StartingLineIndex] = newLine2;
                section.RefreshCode();
            }
        }
    }
}