using System;
using SkEditor.Parser;

namespace SkEditor.Utilities.Parser;

public class ParserException : Exception
{
    public ParserException(string message, Node? node) : base($"Error at line {node?.Line ?? -1}: {message}")
    {
        
    }
}