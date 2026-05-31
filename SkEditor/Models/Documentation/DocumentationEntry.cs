using Avalonia.Media;
using FluentIcons.Common;
using SkEditor.Models.UI;

namespace SkEditor.Models.Documentation;

public record DocumentationEntry
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required string Description { get; init; }
    public required string Addon { get; init; }
    public required string Version { get; init; }
    public required string Patterns { get; init; }

    public StandardIcon Icon => Type switch
    {
        "events" => Symbol.Flash,
        "effects" => Symbol.Wand,
        "expressions" => Symbol.BracesVariable,
        "conditions" => Symbol.ShieldQuestion,
        "types" or "classes" => Symbol.Tag,
        "structures" => Symbol.Organization,
        "sections" => Symbol.AlignLeft,
        "functions" => Symbol.MathFormula,
        _ => Symbol.QuestionCircle
    };

    public string TypeName => Type switch
    {
        "events" => "Event",
        "effects" => "Effect",
        "expressions" => "Expression",
        "conditions" => "Condition",
        "types" or "classes" => "Type",
        "structures" => "Structure",
        "sections" => "Section",
        "functions" => "Function",
        _ => Type
    };

    public Color TypeColor => Type switch
    {
        "events" => Color.Parse("#FFFFE07F"),
        "effects" => Color.Parse("#FFC99EFF"),
        "expressions" => Color.Parse("#FF9DFFA7"),
        "conditions" => Color.Parse("#FFFF5C86"),
        "types" or "classes" => Color.Parse("#FFAFDDFF"),
        "structures" => Color.Parse("#FFB3F691"),
        "sections" => Color.Parse("#FFFF8A73"),
        "functions" => Color.Parse("#FF44CFC2"),
        _ => Color.Parse("#FFC1C1C1")
    };
}