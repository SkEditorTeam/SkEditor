using SkEditor.API;
using SkEditor.Views;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkEditor.Utilities.Parser;

/// <summary>
/// This represent an option reference, e.g. '{@optionName}' and not its declaration, that is <see cref="CodeOption"/>.
/// </summary>
public class CodeOptionReference : INameableCodeElement
{
    public static readonly string OptionReferencePattern = @"{@([a-zA-Z0-9_]+)}";

    public CodeSection Section { get; private set; }

    public string Name { get; private set; }

    public int Line { get; set; }
    public int Column { get; set; }
    public int Length { get; set; }

    public CodeOptionReference(CodeSection section, string raw,
        int line = -1, int column = -1)
    {
        var match = Regex.Match(raw, OptionReferencePattern);
        Section = section;
        Name = match.Groups[1].Value;
        Line = line;
        Column = column;
        Length = raw.Length;
    }

    public override string ToString()
    {
        return $"{{@{Name}}}";
    }

    public bool IsSimilar(CodeOptionReference other)
    {
        return Name == other.Name;
    }

    public bool IsSimilar(CodeOption definition)
    {
        return Name == definition.Name;
    }

    public void Rename(string newName)
    {
        Section.Parser.GetOptionsSection()?.Options.ToList().Find(o => o.Name == Name)?.Rename(newName);
    }

    public async void Rename()
    {
        
    }

    public void Replace(string newName)
    {
        if (newName.StartsWith('{') && newName.EndsWith('}'))
            newName = newName[1..^1];

        Section.Lines[Line - Section.StartingLineIndex - 1] = Section.Lines[Line - Section.StartingLineIndex - 1].Replace(ToString(),
            $"{{@{newName}}}");
        Section.RefreshCode();
    }

    public void NavigateToDefinition()
    {
        var optionsSection = Section.Parser.GetOptionsSection();
        var option = optionsSection?.Options.ToList().Find(o => o.Name == Name);
        if (option != null)
        {
            var editor = Section.Parser.Editor;
            editor.ScrollTo(option.Line, option.Column);
            editor.CaretOffset = editor.Document.GetOffset(option.Line, option.Column);
            editor.Focus();
        }
        else
        {
            SkEditorAPI.Windows.ShowError("The desired option has no definition.");
        }
    }
}