using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AvaloniaEdit;
using SkEditor.API;
using SkEditor.Utilities.Extensions;
using SkEditor.Views;

namespace SkEditor.Utilities.Parser;

/// <summary>
///     This represent an option reference, e.g. '{@optionName}' and not its declaration, that is <see cref="CodeOption" />
///     .
/// </summary>
public class CodeOptionReference : INameableCodeElement
{
    public static readonly string OptionReferencePattern = @"{@([a-zA-Z0-9_]+)}";

    public CodeOptionReference(CodeSection section, string raw,
        int line = -1, int column = -1)
    {
        Match match = Regex.Match(raw, OptionReferencePattern);
        Section = section;
        Name = match.Groups[1].Value;
        Line = line;
        Column = column;
        Length = raw.Length;
    }

    public CodeSection Section { get; }

    public int Line { get; set; }
    public int Column { get; set; }
    public int Length { get; set; }

    public string Name { get; }

    public void Rename(string newName)
    {
        Section.Parser.GetOptionsSection()?.Options.ToList().Find(o => o.Name == Name)?.Rename(newName);
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

    public async Task Rename()
    {
        SymbolRefactorWindow renameWindow = new(this);
        await renameWindow.ShowDialogOnMainWindow();
        Section.Parser.Parse();
    }

    public void Replace(string newName)
    {
        if (newName.StartsWith('{') && newName.EndsWith('}'))
        {
            newName = newName[1..^1];
        }

        Section.Lines[Line - Section.StartingLineIndex - 1] = Section.Lines[Line - Section.StartingLineIndex - 1]
            .Replace(ToString(),
                $"{{@{newName}}}");
        Section.RefreshCode();
    }

    public void NavigateToDefinition()
    {
        CodeSection? optionsSection = Section.Parser.GetOptionsSection();
        CodeOption? option = optionsSection?.Options.ToList().Find(o => o.Name == Name);
        if (option != null)
        {
            TextEditor editor = Section.Parser.Editor;
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