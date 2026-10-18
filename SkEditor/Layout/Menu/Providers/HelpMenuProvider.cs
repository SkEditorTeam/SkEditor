using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Utilities.Localization;
using MainMenuItem = SkEditor.ViewModels.Menu.MainMenuItem;

namespace SkEditor.Utilities.Menu.Providers;

public class HelpMenuProvider(IWindowDialogService windowDialogs, ILinkService links) : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        MainMenuItem helpMenu = MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderHelp);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderAppWiki, Symbol.BookQuestionMark)
            .WithCommand(() => links.OpenLink("https://docs.skeditor.dev/"))
            .AddTo(helpMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCommunitySupport, Symbol.ChatHelp)
            .WithCommand(() => links.OpenLink("https://discord.skeditor.dev/"))
            .AddTo(helpMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderAbout, Symbol.Info)
            .WithCommand(windowDialogs.ShowAboutDialogAsync)
            .AddTo(helpMenu);

        return helpMenu;
    }
}