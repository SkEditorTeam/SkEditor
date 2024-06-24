using System.Collections.Generic;
using SkEditor.Parser;

namespace SkEditor.Parser;

public class ParsingContext
{
    public bool Debug { get; set; }

    public List<(Node, string)> Errors { get; } = new();
    public List<(Node, string)> Warnings { get; } = new();
    
    public List<Node> ParsedNodes { get; } = new();
    
    public void Error(Node node, string message)
    {
        Errors.Add((node, message));
    }
    
    public void Warning(Node node, string message)
    {
        Warnings.Add((node, message));
    }
    
}