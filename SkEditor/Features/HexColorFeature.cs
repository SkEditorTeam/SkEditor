using System.Text.RegularExpressions;
using AvaloniaEdit;
using SkEditor.ElementGenerators;
using SkEditor.Models.Tabs;
using SkEditor.Transformers;
using SkEditor.Utilities.Settings;

namespace SkEditor.Features;

public partial class HexColorFeature(ExperimentsConfig experiments) : IEditorFeature
{
    private readonly HexColorElementGenerator _colorGenerator = new();
    private readonly HexColorTransformer _colorTransformer = new();

    public bool CanAttach(FileTab tab)
    {
        return experiments.HexPreviewEnabled;
    }

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        editor.TextArea.TextView.ElementGenerators.Add(_colorGenerator);
        editor.TextArea.TextView.LineTransformers.Add(_colorTransformer);
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        editor.TextArea.TextView.ElementGenerators.Remove(_colorGenerator);
        editor.TextArea.TextView.LineTransformers.Remove(_colorTransformer);
    }

    [GeneratedRegex(@"<#[#]?(?<hex>(?:[0-9a-fA-F]{3}){1,2})>")]
    public static partial Regex HexRegexGen();
}