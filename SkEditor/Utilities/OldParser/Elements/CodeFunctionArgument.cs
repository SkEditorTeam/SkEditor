using SkEditor.API;
using SkEditor.Views;
using System.Text.RegularExpressions;

namespace SkEditor.Utilities.Parser;

/// <summary>
/// Represent a function argument in a function declaration.
/// </summary>
public class CodeFunctionArgument : INameableCodeElement
{
    public static readonly string FunctionArgumentPattern = "([a-zA-Z0-9_]+):( )?([a-zA-Z0-9_]+)";

    public string Name { get; }
    public string Type { get; }
    public CodeSection Function { get; }
    public int Line { get; set; }
    public int Column { get; set; }
    public int Length { get; set; }

    public CodeFunctionArgument(CodeSection function, string raw, int line = -1, int column = -1)
    {
        var match = Regex.Match(raw, FunctionArgumentPattern);
        Name = match.Groups[1].Value;
        Type = match.Groups[3].Value;

        Function = function;
        Line = line;
        Column = column;
        Length = raw.Length;
    }

    public async void Rename()
    {
        
    }

    public void Rename(string newName)
    {
        // First, rename the function argument in the function declaration
        var functionDeclaration = Function.Lines[0];
        var newFunctionDeclaration = functionDeclaration.Replace(Name, newName);
        Function.Lines[0] = newFunctionDeclaration;

        // Then replace all local variable with the new name
        foreach (CodeVariable variable in Function.Variables)
        {
            if (variable.IsLocal && variable.Name == Name)
                variable.Rename(newName, true);
        }

        Function.RefreshCode();
        Function.Parse();
    }

    public bool IsDefinitionOf(CodeVariable variable)
    {
        return variable.Name == Name && variable.IsLocal;
    }
}