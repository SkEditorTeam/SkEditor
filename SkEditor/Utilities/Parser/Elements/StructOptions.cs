using System.Collections.Generic;
using System.Linq;
using SkEditor.API;
using SkEditor.Parser;
using SkEditor.Parser.Elements;

namespace SkEditor.Utilities.Parser.Elements;

public class StructOptions : Element
{   
    
    public readonly Dictionary<string, string> Options = new();
    
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
                simpleNode.Element = new OptionDefinition(simpleNode.Key, simpleNode.Value);
                
                Options[simpleNode.Key] = simpleNode.Value;
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

    public class OptionDefinition : Element
    {
        public string Key { get; private set; }
        public string Value { get; private set; }
        
        public OptionDefinition(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public override void Load(Node node, ParsingContext context)
        {
            throw new System.NotImplementedException("Cannot load option definition."); 
        }
    }
}