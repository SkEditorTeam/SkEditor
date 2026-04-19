using System.Collections.Generic;
using System.IO;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using SkEditor.Contracts.Services;
using SkEditor.Models.Tabs;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace SkEditor.Features;

public class TextMateFeature(INotificationService notifications) : IEditorFeature
{
    private readonly SkEditorRegistryOptions _registryOptions = new(ThemeName.DarkPlus);
    private TextMate.Installation? _installation;

    public bool CanAttach(FileTab tab) => true;

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        _installation = editor.InstallTextMate(_registryOptions);

        _installation.SetGrammar("source.sk");
        _installation.SetTheme(_registryOptions.GetTheme("dark_skript"));

        editor.TextArea.AddHandler(InputElement.KeyDownEvent, OnEditorPreviewKeyDown, RoutingStrategies.Tunnel,
            handledEventsToo: true);
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
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

        using var stream =
            File.OpenRead(
                @"C:\Users\Notro\Documents\Projekty\CSharp\Aplikacje\SkEditor\SkEditor\Assets\Syntax\skript.tmLanguage.json");
        using var reader = new StreamReader(stream);
        return GrammarReader.ReadGrammarSync(reader);
    }

    public IRawTheme GetTheme(string scopeName)
    {
        if (scopeName != "dark_skript")
        {
            return _defaultRegistry.GetTheme(scopeName);
        }

        using var stream =
            File.OpenRead(
                @"C:\Users\Notro\Documents\Projekty\CSharp\Aplikacje\SkEditor\SkEditor\Assets\Syntax\dark_skript.json");
        using var reader = new StreamReader(stream);
        return ThemeReader.ReadThemeSync(reader);
    }

    public IRawTheme GetDefaultTheme() => _defaultRegistry.GetDefaultTheme();
    public ICollection<string> GetInjections(string scopeName) => _defaultRegistry.GetInjections(scopeName);
    public IRawTheme LoadTheme(ThemeName name) => _defaultRegistry.LoadTheme(name);
}