using System.Collections.Generic;
using System.IO;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
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
    private readonly Dictionary<TextArea, TextMate.Installation> _installations = new();
    private readonly SkEditorRegistryOptions _registryOptions = new(ThemeName.DarkPlus);

    public bool CanAttach(FileTab tab) => true;

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        TextMate.Installation textMateInstallation = editor.InstallTextMate(_registryOptions);

        textMateInstallation.SetGrammar("source.sk");
        textMateInstallation.SetTheme(_registryOptions.GetTheme("dark_skript"));

        _installations.Add(editor.TextArea, textMateInstallation);

        editor.TextArea.AddHandler(InputElement.KeyDownEvent, OnEditorPreviewKeyDown, RoutingStrategies.Tunnel,
            handledEventsToo: true);
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        if (_installations.TryGetValue(editor.TextArea, out var installation))
        {
            installation.Dispose();
            _installations.Remove(editor.TextArea);
        }

        editor.TextArea.RemoveHandler(InputElement.KeyDownEvent, OnEditorPreviewKeyDown);
    }

    private void OnEditorPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.F1)
        {
            return;
        }

        if (sender is not TextArea textArea || !_installations.TryGetValue(textArea, out var installation))
        {
            return;
        }

        notifications.Debug("Reloading TextMate grammar and theme...");

        installation.SetGrammar("source.sk");
        installation.SetTheme(_registryOptions.GetTheme("dark_skript"));
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