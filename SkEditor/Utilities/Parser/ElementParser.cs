using System;
using System.Collections.Generic;
using System.Linq;
using SkEditor.API;

namespace SkEditor.Parser.Elements;

public static class ElementParser
{

    public static void ParseNode(Node node)
    {
        if (node.Element != null) // Has been already parsed, maybe by an element.
            return;

        var registeredElements = Registries.ParserElements.ToList();
        registeredElements.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        registeredElements.Reverse();
        var elements = registeredElements.Select(x => x.Type).ToList();
        
        foreach (var elementType in elements)
        {
            try
            {
                var method = elementType.GetMethod("Parse");
                if (method == null)
                {
                    SkEditorAPI.Logs.Warning($"Element class '{elementType.Name}' does not have a Parse method.");
                    continue;
                }

                if (Activator.CreateInstance(elementType) is not Element instance)
                {
                    SkEditorAPI.Logs.Warning($"Element class '{elementType.Name}' could not be instantiated.");
                    continue;
                }
                
                var result = method.Invoke(null, [node]) as bool?;
                if (result == true)
                {
                    instance.Load(node);
                    node.Element = instance;
                }
            }
            catch (Exception e)
            {
                SkEditorAPI.Logs.Fatal($"An error occurred while parsing node '{node.Key}' at line {node.Line} with element '{elementType.Name}': {e.Message}");
            }
        }
    }

    public static void ParseNodes(Node parent)
    {
        ParseNode(parent);
        if (parent is SectionNode section)
            foreach (var child in section.Children)
                ParseNodes(child);
    }
    
    public static void ParseNodes(IEnumerable<Node> nodes)
    {
        foreach (var node in nodes)
            ParseNodes(node);
    }
    
}