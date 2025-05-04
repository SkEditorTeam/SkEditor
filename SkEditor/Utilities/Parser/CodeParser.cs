using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using AvaloniaEdit;
using SkEditor.API;
using SkEditor.Controls.Sidebar;
using SkEditor.Utilities.InternalAPI;

namespace SkEditor.Utilities.Parser;

public partial class CodeParser : INotifyPropertyChanged
{
    public CodeParser(TextEditor textEditor, bool parse = true)
    {
        Editor = textEditor;
        Sections = [];
        if (parse)
        {
            Parse();
        }
    }

    public static ParserSidebarPanel ParserPanel => AddonLoader.GetCoreAddon().ParserPanel.Panel;

    /// <summary>
    ///     Get the editor that is being parsed.
    /// </summary>
    public TextEditor Editor { get; }

    /// <summary>
    ///     Get the parsed code sections. This will be empty if the code is not parsed yet.
    /// </summary>
    public List<CodeSection> Sections { get; }

    public bool IsParsed { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Get sections with the specified type.
    /// </summary>
    public List<CodeSection> GetSectionFromType(CodeSection.SectionType sectionType)
    {
        return Sections.FindAll(section => section.Type == sectionType);
    }

    /// <summary>
    ///     Get the options section, if any is defined.
    /// </summary>
    public CodeSection? GetOptionsSection()
    {
        return Sections.Find(section => section.Type == CodeSection.SectionType.Options);
    }

    /// <summary>
    ///     Get section from a line.
    /// </summary>
    public CodeSection? GetSectionFromLine(int line)
    {
        return Sections.Find(section => section.ContainsLineIndex(line));
    }

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
        List<string> lines = [.. Editor.Text.Split('\n')];
        int lastSectionLine = -1;
        RemoveComments(ref lines);

        // Parse sections
        for (int lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            string line = lines[lineIndex];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (SectionRegex().IsMatch(line) && !line.StartsWith(' ') && !line.StartsWith('\t') &&
                !line.StartsWith('#'))
            {
                if (lastSectionLine == -1) // Starting
                {
                    lastSectionLine = lineIndex;
                }
                else
                {
                    List<string> linesToParse = lines.GetRange(lastSectionLine, lineIndex - lastSectionLine);
                    Sections.Add(new CodeSection(this, lastSectionLine, linesToParse));
                    lastSectionLine = lineIndex;
                }
            }
        }

        if (lastSectionLine != -1)
        {
            List<string> linesToParse = lines.GetRange(lastSectionLine, lines.Count - lastSectionLine);
            Sections.Add(new CodeSection(this, lastSectionLine, linesToParse));
        }

        if (SkEditorAPI.Core.GetAppConfig().EnableFolding)
        {
            FoldingCreator.CreateFoldings(Editor, Sections);
        }

        ParserPanel.Refresh(Sections);

        ParserPanel.ParseButton.IsEnabled = false;
        ParserPanel.ParseButton.Content = Translation.Get("CodeParserParsed");
    }

    private static void RemoveComments(ref List<string> lines)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            if (line.Contains("###"))
            {
                int index = line.IndexOf("###", StringComparison.Ordinal);
                lines[i] = line[..index];
                for (int j = i + 1; j < lines.Count; j++)
                {
                    if (lines[j].Contains("###"))
                    {
                        int index2 = lines[j].IndexOf("###", StringComparison.Ordinal);
                        lines[j] = lines[j][(index2 + 3)..];
                        break;
                    }

                    lines[j] = "";
                }
            }
            else if (line.Contains('#'))
            {
                int index = line.IndexOf('#');
                lines[i] = line[..index];
            }
        }
    }

    public void SetUnparsed()
    {
        IsParsed = false;
        ParserPanel.ParseButton.IsEnabled = true;
        ParserPanel.ParseButton.Content = Translation.Get("CodeParserParseCode");
        ParserPanel.UpdateInformationBox(true);
    }

    public bool IsValid()
    {
        return !Editor.Text.Any(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t');
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    [GeneratedRegex(@"(.*):")]
    private static partial Regex SectionRegex();
}