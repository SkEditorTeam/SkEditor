using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Projects;
using SettingsWindow = SkEditor.Views.Windows.Settings.SettingsWindow;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Views.Pages;

public partial class WelcomePage : UserControl
{
    public WelcomePage()
    {
        InitializeComponent();

        WelcomeScreenSection gettingStarted = new(
            Translation.GetObservable("WelcomePageGettingStarted"),
            [
                new WelcomeScreenElement(Translation.GetObservable("WelcomePageGettingStartedNewFile"),
                    new RelayCommand(FileHandler.NewFile),
                    CreateSymbolIcon(Symbol.DocumentAdd)),
                new WelcomeScreenElement(Translation.GetObservable("WelcomePageGettingStartedOpenFile"),
                    new AsyncRelayCommand(FileHandler.OpenFile), CreateSymbolIcon(Symbol.DocumentSearch)),
                new WelcomeScreenElement(Translation.GetObservable("WelcomePageGettingStartedOpenFolder"),
                    new AsyncRelayCommand(async () => await ProjectOpener.OpenProject()),
                    CreateSymbolIcon(Symbol.FolderOpen)),
                new WelcomeScreenElement(Translation.GetObservable("WelcomePageGettingStartedSettings"),
                    new AsyncRelayCommand(OpenSettings),
                    CreateSymbolIcon(Symbol.Settings))
            ]
        );

        WelcomeScreenSection help = new(Translation.GetObservable("WelcomePageUsefulLinks"),
        [
            new WelcomeScreenElement(Translation.GetObservable("WelcomePageUsefulLinksDiscordServer"), new RelayCommand(() =>
                SkEditorAPI.Core.OpenLink("https://discord.skeditor.dev")), CreatePathIconSource("DiscordIcon")),

            new WelcomeScreenElement(Translation.GetObservable("WelcomePageUsefulLinksGitHub"), new RelayCommand(() =>
                    SkEditorAPI.Core.OpenLink("https://github.com/SkEditorTeam/SkEditor")),
                CreatePathIconSource("GitHubIcon")),

            new WelcomeScreenElement(Translation.GetObservable("WelcomePageUsefulLinksDocumentation"),
                new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://docs.skeditor.dev")),
                CreateSymbolIcon(Symbol.Book)),
            
            new WelcomeScreenElement(Translation.GetObservable("WelcomePageUsefulLinksSupportUs"),
                new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://ko-fi.com/notro")),
                CreateSymbolIcon(Symbol.Heart))
        ]);

        WelcomeSections.Add(gettingStarted);
        WelcomeSections.Add(help);
    }

    public List<WelcomeScreenSection> WelcomeSections { get; } = [];
    public string Version { get; } = $"v{SkEditorAPI.Core.GetInformationalVersion()}";

    private static SymbolIconSource CreateSymbolIcon(Symbol symbol)
    {
        return new SymbolIconSource
        {
            Symbol = symbol
        };
    }

    private static IconSource? CreatePathIconSource(string name)
    {
        return SkEditorAPI.Core.GetApplicationResource(name) as PathIconSource;
    }

    private static async Task OpenSettings()
    {
        await new SettingsWindow().ShowDialogOnMainWindow();
    }

    public record WelcomeScreenSection(TranslatableString Title, List<WelcomeScreenElement> Entries);

    public record WelcomeScreenElement(TranslatableString Name, IRelayCommand Command, IconSource? Icon);
}