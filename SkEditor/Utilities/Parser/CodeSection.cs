using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AvaloniaEdit.Editing;
using SkEditor.API;

namespace SkEditor.Utilities.Parser;

public class CodeSection
{
    public CodeParser Parser { get; private set; }
    public SectionType Type { get; private set; }
    public List<string> Lines { get; set; }
    public int StartingLineIndex { get; private set; }
    public int EndingLineIndex => StartingLineIndex + Lines.Count;
    
    public HashSet<CodeVariable> GetGlobalVariables() => [..Variables.Where(variable => !variable.IsLocal)];
    public HashSet<CodeVariable> Variables { get; private set; } // Case of any section other than options
    
    public HashSet<CodeOption> Options { get; private set; } // Case of options section
    
    public bool ContainsLineIndex(int line) => line >= StartingLineIndex && line <= EndingLineIndex;
    
    public CodeVariable? GetVariableFromCaret(Caret caret)
    {
        foreach (var variable in Variables)
        {
            if (variable.ContainsCaret(caret))
                return variable;
        }
        return null;
    }
    
    public CodeOption? GetOptionFromCaret(Caret caret)
    {
        foreach (var option in Options)
        {
            if (option.ContainsCaret(caret))
                return option;
        }
        return null;
    }

    public CodeSection(CodeParser parser, int currentLineIndex, List<string> lines)
    {
        Parser = parser;
        Lines = lines;
        StartingLineIndex = currentLineIndex;
        Parse();
    }

    public void Parse()
    {
        // Parse section type
        var firstLine = Lines[0];
        if (firstLine.StartsWith("command") || firstLine.StartsWith("discord command")) 
            Type = SectionType.Command;
        else if (firstLine.Equals("options:"))
            Type = SectionType.Options;
        else if (firstLine.StartsWith("function"))
            Type = SectionType.Function;
        else 
            Type = SectionType.Event;

        
        
        Options = new HashSet<CodeOption>();
        Variables = new HashSet<CodeVariable>();
        
        if (Type == SectionType.Options)
        {
            // Parse options
            for (var index = 1; index <= Lines.Count; index++)
            {
                var line = Lines[index - 1];
                if (line.TrimStart(' ', '\t').StartsWith('#'))
                    continue;

                var matches = Regex.Matches(line, @"(.*): (.*)");
                foreach (var m in matches)
                {
                    var match = m as Match;
                    if (!match.Success)
                        continue;
                    var column = match.Index + 1;
                    var raw = match.Value;
                    Options.Add(new CodeOption(this, raw, StartingLineIndex + index, column));
                }
            }
        }
        else
        {
            // Parse variables
            int lineIndex = StartingLineIndex;
            foreach (var line in Lines)
            {
                var matches = Regex.Matches(line, @"(?<=\{)_?(.*?)(?=\})");
                foreach (var m in matches)
                {
                    var match = m as Match;
                    if (!match.Success)
                        continue;
                    var column = match.Index + 1;
                    var raw = match.Value;
                    Variables.Add(new CodeVariable(this, raw, lineIndex + 1, column));
                }
                lineIndex++;
            }   
        }
    }

    public enum SectionType
    {
        Command,
        Event,
        Options,
        Function
    }

    /// <summary>
    /// Refresh the editor's content with the new code from this section.
    /// This will replace the previous code that were in the lines of this section.
    /// remove the old code.
    /// </summary>
    public void RefreshCode()
    {
        var sectionCode = string.Join("\n", Lines);
        var editor = Parser.Editor;
        var document = editor.Document;
        var startOffset = document.GetOffset(StartingLineIndex+1, 0);
        var endOffset = document.GetOffset(EndingLineIndex, 0);
        if (EndingLineIndex == document.LineCount)
            endOffset = document.TextLength;
        document.Replace(startOffset, endOffset - startOffset, sectionCode);
    }
}