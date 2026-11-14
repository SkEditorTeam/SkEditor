using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkEditor.Editor.Generators.ViewModels;

namespace SkEditor.Editor.Generators;

public static class GuiCodeGenerator
{
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

        AppendVanillaSkript(code, items, title, rows, options.BackgroundItem);
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

        if (item.HideTooltip)
        {
            code.Append(" without entire tooltip");
        }
    }

    public record Options(
        string FunctionName = "openGUI",
        string Title = "GUI",
        int Rows = 3,
        GuiItemViewModel? BackgroundItem = null
    );
}