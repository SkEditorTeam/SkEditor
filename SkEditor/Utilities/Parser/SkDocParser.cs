using AvaloniaEdit;
using AvaloniaEdit.Document;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities.Files;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkEditor.Utilities.Parser;
public partial class SkDocParser
{
    public record SkDocFunction(int Line, string Name, string? ReturnType, List<SkDocParameter> Parameters);
    public record SkDocParameter(string Name, string Type);

    public static Dictionary<CodeParser, List<SkDocFunction>> SkDocFunctions { get; } = [];

    private static readonly Regex _functionRegex = FunctionRegex();

    public static void Parse(CodeParser parser, List<CodeSection> sections)
    {
        SkDocFunctions.Clear();
        sections = sections.Where(x => x.Type == CodeSection.SectionType.Function).ToList();

        foreach (var section in sections)
        {
            int line = section.StartingLineIndex + 1;
            DocumentLine docLine = parser.Editor.Document.GetLineByNumber(line);
            string lineText = parser.Editor.Document.GetText(docLine);

            if (!_functionRegex.IsMatch(lineText)) continue;

            Match match = _functionRegex.Match(lineText);
            string name = match.Groups["name"].Value;
            string parameters = match.Groups["parameters"].Value;
            string returnType = match.Groups["returnType"].Value;
            List<SkDocParameter> parametersList = [];

            if (!string.IsNullOrEmpty(parameters))
            {
                string[] parametersArray = parameters.Split(',');

                foreach (var parameter in parametersArray)
                {
                    string[] parameterParts = parameter.Trim().Split(' ');
                    string parameterType = parameterParts[^1];
                    string parameterName = parameterParts[^2];
                    parametersList.Add(new(parameterName, parameterType));
                }
            }

            SkDocFunction function = new(line, name, returnType, parametersList);

            if (!SkDocFunctions.TryGetValue(parser, out List<SkDocFunction>? value))
            {
                value = [];
                SkDocFunctions[parser] = value;
            }

            value.Add(function);
        }
    }

    public static SkDocFunction? GetFunction(TextEditor editor, int line)
    {
        var file = FileHandler.OpenedFiles.FirstOrDefault(x => x.Editor == editor);
        if (file == null)
            return null;

        CodeParser parser = file.Parser;

        if (!SkDocFunctions.TryGetValue(parser, out var functions))
            return null;

        return functions.FirstOrDefault(f => f.Line == line);
    }

    public static SkDocFunction? GetFunctionFromCall(TextEditor editor, SimpleSegment segment)
    {
        int line = editor.Document.GetLineByOffset(segment.Offset).LineNumber;
        DocumentLine documentLine = editor.Document.GetLineByNumber(line);
        string text = editor.Document.GetText(documentLine);

        var match = FunctionCallRegex().Match(text);
        if (!match.Success) return null;

        string name = match.Groups["name"].Value;
        if (!editor.Document.GetText(segment).Equals(name)) return null;

        var file = FileHandler.OpenedFiles.FirstOrDefault(x => x.Editor == editor);
        if (file == null)
            return null;

        CodeParser parser = file.Parser;

        if (!SkDocFunctions.TryGetValue(parser, out var functions))
            return null;

        return functions.FirstOrDefault(f => f.Name == name);
    }

    [GeneratedRegex(@"(^function|^local function) (?<name>\w+)\((?<parameters>[^)]*)\)(?: :: (?<returnType>\w+))?:")]
    private static partial Regex FunctionRegex();

    [GeneratedRegex(@"\b(?<name>\w+)\((?<parameters>[^)]*)\)")]
    private static partial Regex FunctionCallRegex();
}
