using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SkEditor.API;
using SkEditor.Parser;
using SkEditor.Parser.Elements;

namespace SkEditor.Utilities.Parser.Elements;

/// <summary>
/// Represent an element that can have
/// expressions such as variables or options.
/// 
/// This is meant to centralize the logic
/// of parsing those expressions in a single place.
/// </summary>
public abstract partial class ExprProviderElement : Element
{
    public List<Variable> Variables { get; } = new();
    public List<Option> Options { get; } = new();

    /// <summary>
    /// Parse a given input string to extract expressions.
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <param name="context">The parsing context.</param>
    /// <returns>The parsed input string.</returns>
    /// <remarks>
    /// This method should be called in the Load method
    /// of the derived class.
    /// </remarks>
    protected void ParseExpressions(string input, ParsingContext context)
    {
        SkEditorAPI.Logs.Debug($"Parsing expressions for '{input}'");
        // Parse variables
        foreach (Match match in VariableRegex().Matches(input))
        {
            var name = match.Groups["name"].Value;
            var type = name[0] switch
            {
                '_' => VariableType.Local,
                '-' => VariableType.Memory,
                _ => VariableType.Global
            };
            Variables.Add(new Variable(name.TrimStart('_', '-'), type));
        }
        
        // Parse options
        foreach (Match match in OptionRegex().Matches(input))
        {
            var key = match.Groups["key"].Value;
            Options.Add(new Option(key.TrimStart('@')));
        }
    }
    
    /// <summary>
    /// Represent a variable in an expression.
    /// Example:
    ///     * {_hello} is a local variable named "hello".
    ///     * {hello} is a global variable named "hello".
    ///     * {-hello} is a memory variable named "hello".
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="type">The type of the variable.</param>
    public record Variable(string name, VariableType type);
    
    /// <summary>
    /// Represent an option in an expression.
    /// Example:
    ///     * {@hello} is an option named "hello".
    /// </summary>
    /// <param name="key">The key of the option.</param>
    public record Option(string key);
    
    /// <summary>
    /// The type of variable in an expression.
    /// </summary>
    public enum VariableType
    {
        Local,
        Global,
        Memory
    }

    [GeneratedRegex(@"\{(?<name>[_a-zA-Z-][_a-zA-Z0-9]*)\}")]
    private static partial Regex VariableRegex();
    [GeneratedRegex(@"\{(?<key>@[a-zA-Z0-9]+)\}")]
    private static partial Regex OptionRegex();

    public override string Debug()
    {
        var variables = string.Join(", ", Variables.Select(v => v.type switch
        {
            VariableType.Local => $"_{v.name}",
            VariableType.Memory => $"-{v.name}",
            _ => v.name
        }));
        
        var options = string.Join(", ", Options.Select(o => $"@{o.key}"));
        return $"Variables: {variables}, Options: {options}";
    }
}