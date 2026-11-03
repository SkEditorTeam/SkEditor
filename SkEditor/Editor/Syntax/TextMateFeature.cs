using System.Collections.Generic;
using System.IO;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using R3;
using SkEditor.Editor.Features;
using SkEditor.Editor.Syntax.Models;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Workspace.Tabs.Capabilities;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace SkEditor.Editor.Syntax;

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
        
        fileTab.File.PathChanged
            .Subscribe(_ => InitializeTextMateInstallation(editor, fileTab))
            .AddTo(ref _disposables);

        InitializeTextMateInstallation(editor, fileTab);
    }

    private void InitializeTextMateInstallation(TextEditor editor, IFileTab fileTab)
    {
        _installation?.Dispose();
        _installation = null;
        
        string extension = Path.GetExtension(fileTab.Path)?.ToLowerInvariant() ?? "";

        SkEditorRegistryOptions registryOptions = new(ThemeName.DarkPlus, syntax);

        string? scope = registryOptions.GetScopeByExtension(extension);
        if (string.IsNullOrEmpty(scope))
        {
            return;
        }

        _installation = editor.InstallTextMate(registryOptions);
        _installation.SetGrammar(scope);

        SyntaxPair? pair = syntax.GetPairByExtension(extension);
        IRawTheme? theme = null;

        if (pair != null && !string.IsNullOrEmpty(pair.Theme.FileName))
        {
            theme = registryOptions.LoadThemeById(pair.Theme.FileName);
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
        if (customPair is not null && !string.IsNullOrEmpty(customPair.Grammar?.ScopeName))
        {
            return customPair.Grammar.ScopeName;
        }

        return string.IsNullOrEmpty(normalizedExt)
            ? null
            : _defaultRegistry.GetScopeByExtension(normalizedExt);
    }

    public IRawGrammar? GetGrammar(string scopeName)
    {
        return syntax.GetRawGrammar(scopeName);
    }

    public IRawTheme? LoadThemeById(string themeIdentifier)
    {
        return syntax.GetRawTheme(themeIdentifier);
    }

    public IRawTheme GetTheme(string scopeName)
    {
        return _defaultRegistry.GetTheme(scopeName) ?? syntax.GetDefaultTheme();
    }

    public IRawTheme GetDefaultTheme()
    {
        return syntax.GetDefaultTheme();
    }

    public ICollection<string> GetInjections(string scopeName)
    {
        return _defaultRegistry.GetInjections(scopeName);
    }

    public IRawTheme LoadTheme(ThemeName name)
    {
        return syntax.GetRawTheme(name.ToString()) ?? _defaultRegistry.LoadTheme(name);
    }
}