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
    public required string[] Patterns { get; init; }

    public StandardIcon Icon => Type switch
    {
        "events" => Symbol.Flash,
        "effects" => Symbol.Wand,
        "expressions" => Symbol.BracesVariable,
        "conditions" => Symbol.ShieldQuestion,
        "types" => Symbol.Tag,
        "structures" => Symbol.Organization,
        "sections" => Symbol.AlignLeft,
        "functions" => Symbol.CubeQuick,
        _ => Symbol.QuestionCircle
    };
}