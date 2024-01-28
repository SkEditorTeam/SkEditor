using System.Collections.Generic;
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
        var currentLine = Section.Lines[Line - Section.StartingLineIndex - 1];
        var regex = new Regex(Regex.Escape(Name));
        var newCurrentLine = regex.Replace(currentLine, newName, 1);
        Section.Lines[Line - Section.StartingLineIndex - 1] = newCurrentLine;
        Length = newName.Length;
        Section.RefreshCode();
        
        // Then rename all references
        foreach (var section in Section.Parser.Sections)
        {
            foreach (var reference in section.OptionReferences)
            {
                if (reference.IsSimilar(this))
                {
                    reference.Replace(newName);
                }
            }
        }
    }
}