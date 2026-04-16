using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Platform;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using SkEditor.Models.Tabs;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace SkEditor.Features;

public class TextMateFeature : IEditorFeature
{
    private TextMate.Installation? _textMateInstallation;
    public bool CanAttach(FileTab tab) => true;

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        var registryOptions = new SkEditorRegistryOptions(ThemeName.DarkPlus);
        _textMateInstallation = editor.InstallTextMate(registryOptions);

        _textMateInstallation.SetGrammar("source.sk");
        _textMateInstallation.SetTheme(registryOptions.GetTheme("dark_skript"));
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        _textMateInstallation?.Dispose();
    }
}

public class SkEditorRegistryOptions(ThemeName defaultTheme) : IRegistryOptions
{
    private readonly RegistryOptions _defaultRegistry = new(defaultTheme);

    public IRawGrammar GetGrammar(string scopeName)
    {
        if (scopeName != "source.sk")
        {
            return _defaultRegistry.GetGrammar(scopeName);
        }

        using var stream = AssetLoader.Open(new Uri("avares://SkEditor/Assets/Syntax/skript.tmLanguage.json"));
        using var reader = new StreamReader(stream);
        return GrammarReader.ReadGrammarSync(reader);
    }

    public IRawTheme GetTheme(string scopeName)
    {
        if (scopeName != "dark_skript")
        {
            return _defaultRegistry.GetTheme(scopeName);
        }

        using var stream = AssetLoader.Open(new Uri("avares://SkEditor/Assets/Syntax/dark_skript.json"));
        using var reader = new StreamReader(stream);
        return ThemeReader.ReadThemeSync(reader);
    }

    public IRawTheme GetDefaultTheme() => _defaultRegistry.GetDefaultTheme();
    public ICollection<string> GetInjections(string scopeName) => _defaultRegistry.GetInjections(scopeName);
    public IRawTheme LoadTheme(ThemeName name) => _defaultRegistry.LoadTheme(name);
}