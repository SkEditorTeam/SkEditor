using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs.Capabilities;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Themes.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace SkEditor.Features;

public class TextMateFeature(AppSettings appSettings, INotificationService notifications) : IEditorFeature
{
    private TextMate.Installation? _installation;
    private IRegistryOptions _registryOptions = new SkEditorRegistryOptions(ThemeName.DarkPlus);
    private DisposableBag _disposables;

    public bool CanAttach(IFeatureTab tab)
    {
        return tab is IFileTab;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        _disposables = new DisposableBag();
        
        _installation = editor.InstallTextMate(_registryOptions);

        if (tab is not IFileTab fileTab)
        {
            return;
        }

        string? extension = Path.GetExtension(fileTab.Path ?? string.Empty)?.ToLowerInvariant();

        if (string.IsNullOrEmpty(extension) || extension == ".sk")
        {
            _registryOptions = new SkEditorRegistryOptions(ThemeName.DarkPlus);

            _installation.SetGrammar("source.sk");
            _installation.SetTheme(_registryOptions.GetTheme(appSettings.SyntaxTheme));
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

        appSettings
            .ObservePropertyChanged(x => x.SyntaxTheme)
            .Subscribe(_ => { _installation?.SetTheme(_registryOptions.GetTheme(appSettings.SyntaxTheme)); })
            .AddTo(ref _disposables);
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        _disposables.Dispose();
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
        _installation?.SetTheme(_registryOptions.GetTheme(appSettings.SyntaxTheme));
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
        string[] themes = ["dark_skript", "ethereal_skript"];

        if (!themes.Contains(scopeName))
        {
            return _defaultRegistry.GetTheme(scopeName);
        }

        using Stream stream = AssetLoader.Open(new Uri($"avares://SkEditor/Assets/Syntax/{scopeName}.json"));

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