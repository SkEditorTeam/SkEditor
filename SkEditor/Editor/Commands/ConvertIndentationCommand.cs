using AvaloniaEdit;
using AvaloniaEdit.Document;
using SkEditor.Configuration.Configs;

namespace SkEditor.Editor.Commands;

public class ConvertIndentationCommand(AppSettings appSettings) : IEditorCommandHandler
{
    public EditorCommandType[] HandledCommandTypes =>
    [
        EditorCommandType.SpacesToTabs, EditorCommandType.TabsToSpaces
    ];

    public void Execute(TextDocument document, TextEditor editor, EditorCommandType commandType)
    {
        switch (commandType)
        {
            case EditorCommandType.SpacesToTabs:
            {
                TransformSpacesToTabs(document);
                break;
            }
            case EditorCommandType.TabsToSpaces:
            {
                TransformTabsToSpaces(document);
                break;
            }
        }
    }

    public void TransformSpacesToTabs(TextDocument doc)
    {
        int spacesPerTab = DetectSpaceAmount(doc);

        using (doc.RunUpdate())
        {
            for (int i = doc.LineCount; i >= 1; i--)
            {
                DocumentLine line = doc.GetLineByNumber(i);
                int lineOffset = line.Offset;
                int lineLength = line.Length;

                if (lineLength == 0)
                {
                    continue;
                }

                int scanOffset = 0;

                while (scanOffset < lineLength)
                {
                    char c = doc.GetCharAt(lineOffset + scanOffset);
                    if (c == '\t')
                    {
                        scanOffset++;
                    }
                    else
                    {
                        break;
                    }
                }

                int spaceStartOffset = lineOffset + scanOffset;
                int spaceCount = 0;

                while (scanOffset < lineLength)
                {
                    char c = doc.GetCharAt(lineOffset + scanOffset);
                    if (c == ' ')
                    {
                        spaceCount++;
                        scanOffset++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (spaceCount < spacesPerTab)
                {
                    continue;
                }

                int tabsToInsert = spaceCount / spacesPerTab;
                int totalReplacedSpaces = tabsToInsert * spacesPerTab;

                string newIndent = new('\t', tabsToInsert);
                doc.Replace(spaceStartOffset, totalReplacedSpaces, newIndent);
            }
        }
    }

    public void TransformTabsToSpaces(TextDocument doc)
    {
        int spacesPerTab = appSettings.EditorTabSize > 0 ? appSettings.EditorTabSize : 4;

        using (doc.RunUpdate())
        {
            for (int i = doc.LineCount; i >= 1; i--)
            {
                DocumentLine line = doc.GetLineByNumber(i);
                int lineOffset = line.Offset;
                int lineLength = line.Length;

                if (lineLength == 0)
                {
                    continue;
                }

                int tabCount = 0;
                while (tabCount < lineLength && doc.GetCharAt(lineOffset + tabCount) == '\t')
                {
                    tabCount++;
                }

                if (tabCount == 0)
                {
                    continue;
                }

                int spacesToInsert = tabCount * spacesPerTab;
                string newIndent = new(' ', spacesToInsert);

                doc.Replace(lineOffset, tabCount, newIndent);
            }
        }
    }

    private int DetectSpaceAmount(TextDocument doc)
    {
        int fallback = appSettings.EditorTabSize > 0 ? appSettings.EditorTabSize : 4;

        int count2 = 0;
        int count4 = 0;
        int linesScanned = 0;

        for (int i = 1; i <= doc.LineCount && linesScanned < 100; i++)
        {
            DocumentLine line = doc.GetLineByNumber(i);
            if (line.Length == 0)
            {
                continue;
            }

            int spaces = CountSpacesAfterTabs(doc, line.Offset, line.Length);

            if (spaces <= 1)
            {
                continue;
            }

            linesScanned++;

            if (spaces % 4 == 0)
            {
                count4++;
            }
            else if (spaces % 2 == 0)
            {
                count2++;
            }
        }

        if (count4 > count2 && count4 > 0)
        {
            return 4;
        }

        if (count2 > count4 && count2 > 0)
        {
            return 2;
        }

        return fallback;
    }

    private static int CountSpacesAfterTabs(TextDocument doc, int lineOffset, int lineLength)
    {
        int scan = 0;
        while (scan < lineLength && doc.GetCharAt(lineOffset + scan) == '\t')
        {
            scan++;
        }

        int spaces = 0;
        while (scan < lineLength && doc.GetCharAt(lineOffset + scan) == ' ')
        {
            spaces++;
            scan++;
        }

        return spaces;
    }
}