using System.Collections.Generic;
using System.Text.RegularExpressions;
using AvaloniaEdit.Editing;
using SkEditor.API;

namespace SkEditor.Utilities.Parser;

public class CodeVariable : INameableCodeElement
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
        IsLocal = raw.StartsWith("_");
        Name = raw.TrimStart('_');
        Line = line;
        Column = column;
        Length = raw.Length;
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
    
    public void Rename(string newName)
    {
        if (newName.StartsWith("{") && newName.EndsWith("}")) newName = newName[1..^1];
        if (newName.StartsWith("_")) newName = newName[1..];
        
        if (IsLocal) // rename all within the section
        {
            var current = Section.Lines;
            var newLines = new List<string>();
            foreach (var line in current)
            {
                bool lineAdded = false;
                var matches = Regex.Matches(line, @"(?<=\{)_?(.*?)(?=\})");
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
    }

    public string GetNameDisplay()
    {
        return $"{{{(IsLocal ? "_" : "")}{Name}}}";
    }
}