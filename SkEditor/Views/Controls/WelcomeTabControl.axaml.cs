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

namespace SkEditor.Views.Controls;

public partial class WelcomeTabControl : UserControl
{
    // This is just a little, funny thing serving no real purpose... But it's nice, right?
    private readonly string[] _tooltips = ["Hey!", "Hi!", "How are you?", "What will you code today?"];

    public WelcomeTabControl()
    {
        InitializeComponent();

        WelcomeScreenSection gettingStarted = new(
            Translation.GetObservable("WelcomeTabGettingStarted"),
            [
                new WelcomeScreenElement(Translation.GetObservable("WelcomeGettingStartedNewFile"),
                    new RelayCommand(FileHandler.NewFile),
                    CreateSymbolIcon(Symbol.DocumentAdd)),
                new WelcomeScreenElement(Translation.GetObservable("WelcomeGettingStartedOpenFile"),
                    new AsyncRelayCommand(FileHandler.OpenFile), CreateSymbolIcon(Symbol.DocumentSearch)),
                new WelcomeScreenElement(Translation.GetObservable("WelcomeGettingStartedOpenFolder"),
                    new AsyncRelayCommand(async () => await ProjectOpener.OpenProject()),
                    CreateSymbolIcon(Symbol.FolderOpen)),
                new WelcomeScreenElement(Translation.GetObservable("WelcomeGettingStartedSettings"),
                    new AsyncRelayCommand(OpenSettings),
                    CreateSymbolIcon(Symbol.Settings))
            ]
        );

        WelcomeScreenSection help = new(Translation.GetObservable("WelcomeTabNeedHelp"),
        [
            new WelcomeScreenElement(Translation.GetObservable("WelcomeNeedHelpDiscordServer"), new RelayCommand(() =>
                SkEditorAPI.Core.OpenLink("https://discord.skeditor.dev")), CreatePathIconSource("DiscordIcon")),

            new WelcomeScreenElement(Translation.GetObservable("WelcomeNeedHelpGitHub"), new RelayCommand(() =>
                    SkEditorAPI.Core.OpenLink("https://github.com/SkEditorTeam/SkEditor")),
                CreatePathIconSource("GitHubIcon")),

            new WelcomeScreenElement(Translation.GetObservable("WelcomeNeedHelpDocumentation"),
                new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://docs.skeditor.dev")),
                CreateSymbolIcon(Symbol.Book)),
            
            new WelcomeScreenElement("Support SkEditor",
                new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://ko-fi.com/notro")),
                CreateSymbolIcon(Symbol.Heart))
        ]);

        WelcomeSections.Add(gettingStarted);
        WelcomeSections.Add(help);

        ToolTip.SetShowDelay(SkEditorIcon, 500);
        SkEditorIcon.PointerEntered += (_, _) =>
        {
            int index = new Random().Next(_tooltips.Length);
            ToolTip.SetTip(SkEditorIcon, _tooltips[index]);
        };
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