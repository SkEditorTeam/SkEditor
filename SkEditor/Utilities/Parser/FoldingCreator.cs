using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using AvaloniaEdit.Utils;
using SkEditor.Utilities.Files;
using static SkEditor.Utilities.Parser.CodeSection;

namespace SkEditor.Utilities.Parser;

public static class FoldingCreator
{
    public static void CreateFoldings(TextEditor editor, List<CodeSection> sections)
    {
        List<FoldingSection> foldedSections = GetOldFoldedSections(editor);

        FoldingManager foldingManager = FoldingManager.Install(editor.TextArea);
        StyleMarkers(editor);

        foreach (CodeSection section in sections)
        {
            DocumentLine? start = editor.Document.GetLineByNumber(section.StartingLineIndex + 1);
            DocumentLine? end = editor.Document.GetLineByNumber(section.EndingLineIndex);

            while (string.IsNullOrWhiteSpace(editor.Document.GetText(end).Trim()))
            {
                end = editor.Document.GetLineByNumber(end.LineNumber - 1);
            }

            FoldingSection foldingSection = foldingManager.CreateFolding(start.Offset, end.EndOffset);

            foldingSection.IsFolded = foldedSections.Any(fs => fs.StartOffset == foldingSection.StartOffset
                                                               && fs.EndOffset == foldingSection.EndOffset);

            foldingSection.Title = GetTitle(section);
        }
    }

    private static string GetTitle(CodeSection section)
    {
        return section.Type switch
        {
            SectionType.Command => $"Command {section.Name}",
            SectionType.Event => $"Event '{section.Name}'",
            SectionType.Function => $"Function '{section.Name}'",
            SectionType.Options => "Options",
            _ => "..."
        };
    }

    private static void StyleMarkers(TextEditor editor)
    {
        FoldingMargin? margin = editor.TextArea.LeftMargins.OfType<FoldingMargin>().FirstOrDefault();
        if (margin == null)
        {
            return;
        }

        margin.SetValue(FoldingMargin.FoldingMarkerBackgroundBrushProperty,
            new SolidColorBrush(Color.Parse("#27282a")));
        margin.SetValue(FoldingMargin.FoldingMarkerBrushProperty, new SolidColorBrush(Color.Parse("#313234")));
        margin.SetValue(FoldingMargin.SelectedFoldingMarkerBackgroundBrushProperty,
            new SolidColorBrush(Color.Parse("#3f4042")));
        margin.SetValue(FoldingMargin.SelectedFoldingMarkerBrushProperty, new SolidColorBrush(Color.Parse("#939395")));
    }

    private static List<FoldingSection> GetOldFoldedSections(TextEditor editor)
    {
        FoldingManager? oldFoldingManager = editor.GetService<FoldingManager>();

        List<FoldingSection> foldedSections = [];
        if (oldFoldingManager != null)
        {
            foldedSections = oldFoldingManager.AllFoldings.Where(folding => folding.IsFolded).ToList();
            FoldingManager.Uninstall(oldFoldingManager);
        }

        return foldedSections;
    }

    public static List<int> GetHiddenLines(OpenedFile file)
    {
        if (file.Editor == null)
        {
            return [];
        }

        List<int> hiddenLines = [];
        FoldingManager? foldingManager = file.Editor.GetService<FoldingManager>();

        if (foldingManager != null)
        {
            foreach (FoldingSection folding in foldingManager.AllFoldings)
            {
                if (folding.IsFolded)
                {
                    List<int> ls = new();
                    for (int i = folding.StartOffset; i < folding.EndOffset; i++)
                    {
                        int l = file.Editor.Document.GetLineByOffset(i).LineNumber;
                        if (!ls.Contains(l))
                        {
                            ls.Add(l);
                        }
                    }

                    ls.RemoveAt(0); // Remove the first line of the section
                    hiddenLines.AddRange(ls);
                }
            }
        }

        return hiddenLines.Distinct().ToList();
    }
}