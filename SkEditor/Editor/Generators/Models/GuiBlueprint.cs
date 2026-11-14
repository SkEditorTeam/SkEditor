using System.Collections.Generic;

namespace SkEditor.Editor.Generators.Models;

public class GuiBlueprint
{
    public string? Title { get; set; }
    public string? FunctionName { get; set; }
    public required int Rows { get; set; }

    public GuiItem? BackgroundItem { get; set; }
    public required Dictionary<int, GuiItem> Items { get; set; }
}

public class GuiItem
{
    public required string Id { get; set; }
    public string? DisplayName { get; set; }
    public int Amount { get; set; }
    public bool GenerateExampleAction { get; set; }
    public bool HideTooltip { get; set; }
}