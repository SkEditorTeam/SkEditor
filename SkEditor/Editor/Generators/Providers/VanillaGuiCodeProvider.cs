using System.Linq;
using System.Text;
using SkEditor.Editor.Generators.Models;

namespace SkEditor.Editor.Generators.Providers;

public class VanillaGuiCodeProvider : IGuiCodeProvider
{
    public string Name => "Vanilla";
    
    public string Generate(GuiBlueprint blueprint)
    {
        StringBuilder code = new();

        string functionName = blueprint.FunctionName ?? "openGui";
        string title = blueprint.Title ?? "GUI";
        code.Append($"function {functionName}(p: player):");

        code.Append($"\n\tset {{_gui}} to chest inventory with {blueprint.Rows} rows named \"{title}\"\n");

        if (blueprint.BackgroundItem != null)
        {
            int maxSlot = (blueprint.Rows * 9) - 1;
            code.Append($"\n\tset slot (integers between 0 and {maxSlot}) of {{_gui}} to ");
            AppendItemString(code, blueprint.BackgroundItem);
        }

        var sortedItems = blueprint.Items
            .OrderBy(kvp => kvp.Key)
            .ToList();

        foreach ((int slot, GuiItem item) in sortedItems)
        {
            code.Append($"\n\tset slot {slot} of {{_gui}} to ");
            AppendItemString(code, item);
        }

        code.Append("\n\topen {_gui} to {_p}\n");

        code.Append("\non inventory click:");
        code.Append($"\n\tname of event-inventory is \"{blueprint.Title}\"");
        code.Append("\n\tcancel event");
        code.Append("\n\tevent-inventory is not player's inventory");

        bool first = true;
        foreach ((int slot, GuiItem item) in sortedItems)
        {
            if (!item.GenerateExampleAction)
            {
                continue;
            }
            
            code.Append($"\n\t{(first ? "" : "else ")}if clicked slot is {slot}:");
            code.Append($"\n\t\tsend \"You clicked on slot {slot}\" to player");
            first = false;
        }
        
        return code.ToString();
    }

    private static void AppendItemString(StringBuilder code, GuiItem item)
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
}