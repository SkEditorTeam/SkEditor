using System.Collections.Generic;
using System.IO;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Models.UI;
using SkEditor.ViewModels.Tabs.Capabilities;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace SkEditor.Features;

public class TextMateFeature(ISyntaxService syntax) : IEditorFeature
{
    private DisposableBag _disposables;
    private TextMate.Installation? _installation;

    public bool CanAttach(IFeatureTab tab)
    {
        return tab is IFileTab;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        _disposables = new DisposableBag();

        if (tab is not IFileTab fileTab)
        {
            return;
        }

        string extension = Path.GetExtension(fileTab.Path)?.ToLowerInvariant() ?? "";

        SkEditorRegistryOptions registryOptions = new(ThemeName.DarkPlus, syntax);
        _installation = editor.InstallTextMate(registryOptions);

        string? scope = registryOptions.GetScopeByExtension(extension);
        if (string.IsNullOrEmpty(scope))
        {
            return;
        }

        _installation.SetGrammar(scope);

        SyntaxPair? pair = syntax.GetPairByExtension(extension);
        IRawTheme? theme = null;

        if (pair != null && !string.IsNullOrEmpty(pair.Theme.FileName))
        {
            theme = registryOptions.LoadCustomTheme(pair.Theme.FileName);
        }

        _installation.SetTheme(theme ?? registryOptions.GetDefaultTheme());
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        _disposables.Dispose();
        _installation?.Dispose();
    }
}

public class SkEditorRegistryOptions(ThemeName defaultTheme, ISyntaxService syntax) : IRegistryOptions
{
    private readonly RegistryOptions _defaultRegistry = new(defaultTheme);

    public string? GetScopeByExtension(string? extension)
    {
        string normalizedExt = extension?.ToLowerInvariant() ?? "";

        SyntaxPair? customPair = syntax.GetPairByExtension(normalizedExt);
        if (customPair is not null)
        {
            return customPair.Grammar.ScopeName;
        }

        return string.IsNullOrEmpty(normalizedExt)
            ? null
            : _defaultRegistry.GetScopeByExtension(normalizedExt);
    }

    public IRawGrammar? GetGrammar(string scopeName)
    {
        string? grammarFile = syntax.GetGrammarFileByScope(scopeName);
        if (string.IsNullOrEmpty(grammarFile))
        {
            return _defaultRegistry.GetGrammar(scopeName);
        }

        using Stream? stream = syntax.GetGrammarStream(grammarFile);
        if (stream == null)
        {
            return _defaultRegistry.GetGrammar(scopeName);
        }

        using StreamReader reader = new(stream);
        return GrammarReader.ReadGrammarSync(reader);
    }

    public IRawTheme? LoadCustomTheme(string themeFileName)
    {
        using Stream? stream = syntax.GetThemeStream(themeFileName);
        if (stream == null)
        {
            return null;
        }

        using StreamReader reader = new(stream);
        return ThemeReader.ReadThemeSync(reader);
    }

    public IRawTheme GetTheme(string scopeName)
    {
        return _defaultRegistry.GetTheme(scopeName) ?? _defaultRegistry.GetDefaultTheme();
    }

    public IRawTheme GetDefaultTheme()
    {
        return _defaultRegistry.GetDefaultTheme();
    }

    public ICollection<string> GetInjections(string scopeName)
    {
        return _defaultRegistry.GetInjections(scopeName);
    }

    public IRawTheme LoadTheme(ThemeName name)
    {
        return _defaultRegistry.LoadTheme(name);
    }
}