using AvaloniaEdit;
using SkEditor.API;
using SkEditor.Controls.Sidebar;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkEditor.Utilities.Parser;

public class CodeParser : INotifyPropertyChanged
{

    public static ParserSidebarPanel ParserPanel =>
        ApiVault.Get().GetMainWindow().SideBar.ParserPanel.Panel;

    public CodeParser(TextEditor textEditor, bool parse = true)
    {
        Editor = textEditor;
        Sections = new List<CodeSection>();
        if (parse)
            Parse();
    }

    /// <summary>
    /// Get the editor that is being parsed.
    /// </summary>
    public TextEditor Editor { get; private set; }

    /// <summary>
    /// Get the parsed code sections. This will be empty if the code is not parsed yet.
    /// </summary>
    public List<CodeSection> Sections { get; private set; }

    /// <summary>
    /// Get sections with the specified type.
    /// </summary>
    /// <param name="sectionType"></param>
    /// <returns></returns>
    public List<CodeSection> GetSectionFromType(CodeSection.SectionType sectionType) => Sections.FindAll(section => section.Type == sectionType);

    /// <summary>
    /// Get the options section, if any is defined.
    /// </summary>
    /// <returns></returns>
    public CodeSection? GetOptionsSection() => Sections.Find(section => section.Type == CodeSection.SectionType.Options);

    public bool IsParsed { get; private set; } = false;

    /// <summary>
    /// Get section from a line.
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public CodeSection? GetSectionFromLine(int line) => Sections.Find(section => section.ContainsLineIndex(line));

    public void Parse()
    {
        Sections.Clear();
        if (!IsValid())
        {
            SetUnparsed();
            return;
        }

        IsParsed = true;

        // Split the code into lines
        List<string> lines = Editor.Text.Split('\n').ToList();
        int lastSectionLine = -1;

        // Parse sections
        for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            var line = lines[lineIndex];
            if (line.Trim().Length == 0)
                continue;

            if (Regex.IsMatch(line, @"(.*):") && !line.StartsWith(" ") && !line.StartsWith("\t") && !line.StartsWith("#"))
            {
                if (lastSectionLine == -1) // Starting
                {
                    lastSectionLine = lineIndex;
                }
                else
                {
                    var linesToParse = lines.GetRange(lastSectionLine, lineIndex - lastSectionLine);
                    Sections.Add(new CodeSection(this, lastSectionLine, linesToParse));
                    lastSectionLine = lineIndex;
                }
            }
        }

        // Parse the last section
        if (lastSectionLine != -1)
        {
            var linesToParse = lines.GetRange(lastSectionLine, lines.Count - lastSectionLine);
            Sections.Add(new CodeSection(this, lastSectionLine, linesToParse));
        }

        ParserPanel.Refresh(Sections);

        ParserPanel.ParseButton.IsEnabled = false;
        ParserPanel.ParseButton.Content = "Current code parsed";
    }

    public void SetUnparsed()
    {
        IsParsed = false;
        ParserPanel.ParseButton.IsEnabled = true;
        ParserPanel.ParseButton.Content = "Parse code";
        ParserPanel.UpdateInformationBox(true);
    }

    public bool IsValid()
    {
        return !Editor.Text.Any(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t');
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}