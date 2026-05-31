using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using SkEditor.Contracts.Services;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace SkEditor.Features;

public class TextMateFeature(INotificationService notifications) : IEditorFeature
{
    private TextMate.Installation? _installation;
    private IRegistryOptions _registryOptions = new SkEditorRegistryOptions(ThemeName.DarkPlus);

    public bool CanAttach(IFeatureTab tab)
    {
        return true;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        _installation = editor.InstallTextMate(_registryOptions);

        string? extension = tab.GetExtension();

        if (extension is null or ".sk")
        {
            _registryOptions = new SkEditorRegistryOptions(ThemeName.DarkPlus);

            _installation.SetGrammar("source.sk");
            _installation.SetTheme(_registryOptions.GetTheme("dark_skript"));
        }
        else
        {
            RegistryOptions registryOptions = new(ThemeName.DarkPlus);
            _registryOptions = registryOptions;
            string? scope = registryOptions.GetScopeByExtension(extension);
            if (scope is not null)
            {
                _installation.SetGrammar(scope);
                _installation.SetTheme(registryOptions.GetDefaultTheme());
            }
        }

        editor.TextArea.AddHandler(InputElement.KeyDownEvent, OnEditorPreviewKeyDown, RoutingStrategies.Tunnel,
            true);
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        _installation?.Dispose();

        editor.TextArea.RemoveHandler(InputElement.KeyDownEvent, OnEditorPreviewKeyDown);
    }

    private void OnEditorPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.F1)
        {
            return;
        }

        notifications.Debug("Reloading TextMate grammar and theme...");

        _installation?.SetGrammar("source.sk");
        _installation?.SetTheme(_registryOptions.GetTheme("dark_skript"));
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

        using Stream stream = AssetLoader.Open(new Uri("avares://SkEditor/Assets/Syntax/skript.tmLanguage.json"));
        using StreamReader reader = new(stream);
        return GrammarReader.ReadGrammarSync(reader);
    }

    public IRawTheme GetTheme(string scopeName)
    {
        if (scopeName != "dark_skript")
        {
            return _defaultRegistry.GetTheme(scopeName);
        }

        using Stream stream = AssetLoader.Open(new Uri("avares://SkEditor/Assets/Syntax/dark_skript.json"));
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