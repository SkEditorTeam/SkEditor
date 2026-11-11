using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkEditor.Editor.Generators.ViewModels;

namespace SkEditor.Editor.Generators;

public static class GuiCodeGenerator
{
    private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string Generate(IReadOnlyDictionary<int, GuiItemViewModel> items, Options? options = null)
    {
        options ??= new Options();

        string functionName = string.IsNullOrWhiteSpace(options.FunctionName)
            ? "openGUI"
            : options.FunctionName.Trim().Replace(' ', '_');

        string title = string.IsNullOrWhiteSpace(options.Title) ? "GUI" : options.Title;
        int rows = Math.Clamp(options.Rows, 1, 6);

        StringBuilder code = new();
        code.Append($"function {functionName}(p: player):");

        if (!options.UseSkriptGui)
        {
            AppendVanillaSkript(code, items, title, rows, options.BackgroundItem);
        }
        else
        {
            AppendSkriptGui(code, items, title, rows, options.BackgroundItem);
        }

        return code.ToString();
    }

    private static void AppendVanillaSkript(
        StringBuilder code,
        IReadOnlyDictionary<int, GuiItemViewModel> items,
        string title,
        int rows,
        GuiItemViewModel? backgroundItem)
    {
        code.Append($"\n\tset {{_gui}} to chest inventory with {rows} rows named \"{title}\"\n");

        if (backgroundItem != null)
        {
            int maxSlot = (rows * 9) - 1;
            code.Append($"\n\tset slot (integers between 0 and {maxSlot}) of {{_gui}} to ");
            AppendItemString(code, backgroundItem);
        }

        var sortedItems = items
            .Where(kvp => !kvp.Value.IsBackground)
            .OrderBy(kvp => kvp.Key)
            .ToList();

        foreach ((int slot, GuiItemViewModel item) in sortedItems)
        {
            code.Append($"\n\tset slot {slot} of {{_gui}} to ");
            AppendItemString(code, item);
        }

        code.Append("\n\topen {_gui} to {_p}\n");

        code.Append("\non inventory click:");
        code.Append($"\n\tname of event-inventory is \"{title}\"");
        code.Append("\n\tcancel event");
        code.Append("\n\tevent-inventory is not player's inventory");

        bool first = true;
        foreach ((int slot, GuiItemViewModel item) in sortedItems)
        {
            if (!item.GenerateExampleAction)
            {
                continue;
            }
            
            code.Append($"\n\t{(first ? "" : "else ")}if clicked slot is {slot}:");
            code.Append($"\n\t\tsend \"You clicked on slot {slot}\" to player");
            first = false;
        }
    }

    private static void AppendSkriptGui(
        StringBuilder code,
        IReadOnlyDictionary<int, GuiItemViewModel> items,
        string title,
        int rows,
        GuiItemViewModel? backgroundItem)
    {
        code.Append($"\n\tcreate a gui with virtual chest inventory with {rows} rows named \"{title}\"");

        if (backgroundItem != null)
        {
            AppendShapeCode(code, items, rows);
        }

        code.Append(':');

        if (backgroundItem != null)
        {
            code.Append("\n\t\tmake gui slot \"=\" with ");
            AppendItemString(code, backgroundItem);
        }

        IOrderedEnumerable<KeyValuePair<int, GuiItemViewModel>> sortedItems = items
            .Where(kvp => !kvp.Value.IsBackground)
            .OrderBy(kvp => kvp.Key);

        foreach ((int slot, GuiItemViewModel item) in sortedItems)
        {
            code.Append($"\n\t\tmake gui slot {slot} with ");
            AppendItemString(code, item);

            if (item.GenerateExampleAction)
            {
                code.Append(':');
                code.Append($"\n\t\t\tsend \"You clicked on slot {slot}\" to player");
            }
        }

        code.Append("\n\topen the last gui for {_p}");
    }

    private static void AppendShapeCode(StringBuilder code, IReadOnlyDictionary<int, GuiItemViewModel> items, int rows)
    {
        int totalSlots = rows * 9;
        List<List<char>> rowChars = [.. Enumerable.Range(0, rows).Select(_ => new List<char>(9))];
        HashSet<char> usedChars = ['='];

        for (int i = 0; i < totalSlots; i++)
        {
            int rowIndex = i / 9;

            if (!items.TryGetValue(i, out GuiItemViewModel? item) || item.IsBackground)
            {
                rowChars[rowIndex].Add('=');
            }
            else
            {
                char firstChar = !string.IsNullOrEmpty(item.Name) ? item.Name[0] : 'x';
                char chosenChar = !usedChars.Contains(firstChar)
                    ? firstChar
                    : GetRandomChar(usedChars);

                usedChars.Add(chosenChar);
                rowChars[rowIndex].Add(chosenChar);
            }
        }

        code.Append(" and shape");

        if (rows == 1)
        {
            code.Append($" \"{string.Concat(rowChars[0])}\"");
        }
        else
        {
            for (int r = 0; r < rows; r++)
            {
                string rowStr = string.Concat(rowChars[r]);
                code.Append(r == rows - 1 ? $" and \"{rowStr}\"" : $" \"{rowStr}\",");
            }
        }
    }

    private static void AppendItemString(StringBuilder code, GuiItemViewModel item)
    {
        string rawName = item.Id.Replace('_', ' ');

        if (item.Amount > 1)
        {
            code.Append($"{item.Amount} of {rawName}");
        }
        else
        {
            code.Append(rawName);
        }

        if (!string.IsNullOrWhiteSpace(item.DisplayName))
        {
            code.Append($" named \"{item.DisplayName}\"");
        }
    }

    private static char GetRandomChar(HashSet<char> usedChars)
    {
        char c;
        do
        {
            c = Chars[Random.Shared.Next(Chars.Length)];
        } while (usedChars.Contains(c));

        return c;
    }

    public record Options(
        string FunctionName = "openGUI",
        string Title = "GUI",
        int Rows = 3,
        bool UseSkriptGui = false,
        GuiItemViewModel? BackgroundItem = null
    );
}