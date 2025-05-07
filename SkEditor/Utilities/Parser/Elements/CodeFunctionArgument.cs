using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SkEditor.Utilities.Extensions;
using SkEditor.Views;

namespace SkEditor.Utilities.Parser;

/// <summary>
///     Represent a function argument in a function declaration.
/// </summary>
public partial class CodeFunctionArgument : INameableCodeElement
{
    public const string FunctionArgumentPattern = "([a-zA-Z0-9_]+):( )?([a-zA-Z0-9_]+)";

    public CodeFunctionArgument(CodeSection function, string raw, int line = -1, int column = -1)
    {
        Match match = FunctionArgumentRegex().Match(raw);
        Name = match.Groups[1].Value;
        Type = match.Groups[3].Value;

        Function = function;
        Line = line;
        Column = column;
        Length = raw.Length;
    }

    public string Type { get; }
    public CodeSection Function { get; }
    public int Line { get; set; }
    public int Column { get; set; }
    public int Length { get; set; }

    public string Name { get; }

    public void Rename(string newName)
    {
        // First, rename the function argument in the function declaration
        string functionDeclaration = Function.Lines[0];
        string newFunctionDeclaration = functionDeclaration.Replace(Name, newName);
        Function.Lines[0] = newFunctionDeclaration;

        // Then replace all local variable with the new name
        foreach (CodeVariable variable in
                 Function.Variables.Where(variable => variable.IsLocal && variable.Name == Name))
        {
            variable.Rename(newName, true);
        }

        Function.RefreshCode();
        Function.Parse();
    }

    public async Task Rename()
    {
        SymbolRefactorWindow renameWindow = new(this);
        await renameWindow.ShowDialogOnMainWindow();
        Function.Parser.Parse();
    }

    public bool IsDefinitionOf(CodeVariable variable)
    {
        return variable.Name == Name && variable.IsLocal;
    }

    [GeneratedRegex(FunctionArgumentPattern)]
    private static partial Regex FunctionArgumentRegex();
}