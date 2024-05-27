using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SkEditor.API;
using SkEditor.Parser;
using SkEditor.Parser.Elements;

namespace SkEditor.Utilities.Parser.Elements;

public partial class StructCommand : Element
{
    
    public string Name { get; set; }
    
    public List<string>? Aliases { get; set; }
    public string? Permission { get; set; }
    public string? PermissionMessage { get; set; }
    public string? Description { get; set; }
    public string? Prefix { get; set; }
    
    public string? Cooldown { get; set; }
    public string? CooldownMessage { get; set; }
    public string? CooldownBypass { get; set; }
    public string? CooldownStorage { get; set; }
    
    public string? Usage { get; set; }
    
    public override void Load(Node node)
    {
        var section = (SectionNode) node;

        // Parse the name
        var match = CommandRegex().Match(node.Key);
        if (!match.Success)
            throw new ParserException("Invalid command name format.", node);
        
        Name = match.Groups[1].Value;
        
        // Parse simple entries
        Aliases = section.GetSimpleChild("aliases")?.GetAsArray().ToList() ?? [];
        Permission = section.GetSimpleChild("permission")?.Value;
        PermissionMessage = section.GetSimpleChild("permission message")?.Value;
        Description = section.GetSimpleChild("description")?.Value;
        Prefix = section.GetSimpleChild("prefix")?.Value;
        Cooldown = section.GetSimpleChild("cooldown")?.Value;
        CooldownMessage = section.GetSimpleChild("cooldown message")?.Value;
        CooldownBypass = section.GetSimpleChild("cooldown bypass")?.Value;
        CooldownStorage = section.GetSimpleChild("cooldown storage")?.Value;
        Usage = section.GetSimpleChild("usage")?.Value;
        
        // Parse trigger
        ElementParser.ParseNode(section.GetSectionChild("trigger"));
    }

    public static bool Parse(Node node)
    {
        return node is SectionNode
            && node.Key.StartsWith("command")
            && node.Indent == 0;
    }

    [GeneratedRegex("(?i)^command\\s+/?(\\S+)\\s*(\\s+(.+))?$", RegexOptions.None, "fr-FR")]
    private static partial Regex CommandRegex();
}