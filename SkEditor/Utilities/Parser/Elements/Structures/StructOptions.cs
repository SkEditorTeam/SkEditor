using System.Collections.Generic;
using System.Linq;
using SkEditor.API;
using SkEditor.Parser;
using SkEditor.Parser.Elements;

namespace SkEditor.Parser.Elements;

public class StructOptions : Element
{   
    
    public readonly List<OptionDefinition> Options = new();
    
    public override void Load(Node node, ParsingContext context)
    {
        var anotherOptions = context.ParsedNodes.FirstOrDefault(x => x.Element is StructOptions);
        if (anotherOptions != null && anotherOptions != node)
        {
            SkEditorAPI.Logs.Debug("Found multiple options sections.");
            context.Warning(node, "Only one options section is allowed. Found another one at <line:" + anotherOptions.Line + ">.");
        }
        
        foreach (var optionNode in node as SectionNode)
        {
            if (optionNode.IsSimple)
            {
                var simpleNode = optionNode as SimpleNode;
                var option = new OptionDefinition(simpleNode.Key, simpleNode.Value);
                
                Options.Add(option);
                simpleNode.Element = option;
            }
            else
            {
                context.Warning(optionNode, "Option node is not simple ('key: value').");
            }
        }
    }

    public static bool Parse(Node node)
    {
        return node is { 
            IsSection: true, 
            Key: "options", 
            IsTopLevel: true 
        };
    }

    public class OptionDefinition(string key, string value) : Element
    {
        public string Key { get; private set; } = key;
        public string Value { get; private set; } = value;

        public override void Load(Node node, ParsingContext context)
        {
            throw new System.NotImplementedException("Cannot load option definition."); 
        }
    }

    public override string Debug()
    {
        return "Options[" + Options.Count + "]:" +
               string.Join("\n  - ", Options.Select(x => x.Key + ": " + x.Value));
    }
}