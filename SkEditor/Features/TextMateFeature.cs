using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Platform;
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

        string? extension = Path.GetExtension(fileTab.Path)?.ToLowerInvariant();

        SkEditorRegistryOptions registryOptions = new(ThemeName.DarkPlus, syntax);
        _installation = editor.InstallTextMate(registryOptions);

        string? scope = registryOptions.GetScopeByExtension(extension);

        if (scope is null)
        {
            return;
        }

        _installation.SetGrammar(scope);

        SyntaxPair? pair = syntax.GetPairByExtension(extension);
        string? themeName = pair?.Theme.FileName;

        _installation.SetTheme(!string.IsNullOrEmpty(themeName)
            ? registryOptions.GetTheme(themeName)
            : registryOptions.GetDefaultTheme());
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
        SyntaxPair? customPair = syntax.GetPairByExtension(extension);
        if (customPair is not null)
        {
            return customPair.Grammar.FileName;
        }

        return string.IsNullOrEmpty(extension)
            ? null
            : _defaultRegistry.GetScopeByExtension(extension);
    }

    public IRawGrammar GetGrammar(string scopeName)
    {
        if (scopeName != "source.sk")
        {
            return _defaultRegistry.GetGrammar(scopeName);
        }

        using Stream stream = AssetLoader.Open(new Uri("avares://SkEditor/Assets/Syntax/skript.tmLanguage.json"));
        using StreamReader reader = new(stream);
        return GrammarReader.ReadGrammarSync(reader);
    }

    public IRawTheme GetTheme(string scopeName)
    {
        Uri uri = new($"avares://SkEditor/Assets/Syntax/{scopeName}.json");

        if (!AssetLoader.Exists(uri))
        {
            return _defaultRegistry.GetTheme(scopeName) ?? _defaultRegistry.GetDefaultTheme();
        }

        using Stream stream = AssetLoader.Open(uri);
        using StreamReader reader = new(stream);
        return ThemeReader.ReadThemeSync(reader);
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