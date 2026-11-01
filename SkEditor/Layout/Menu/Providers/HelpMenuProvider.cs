using FluentIcons.Common;
using SkEditor.Core.Localization;
using SkEditor.Core.System;
using SkEditor.Core.Updates;
using SkEditor.Core.Updates.Models;
using SkEditor.Layout.Menu.Models;
using SkEditor.Windowing;

namespace SkEditor.Layout.Menu.Providers;

public class HelpMenuProvider(IWindowDialogService windowDialogs, ILinkService links, IUpdateService updates) : IMenuProvider
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
        
        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCheckForUpdates, Symbol.ArrowSync)
            .WithCommand(async () =>
            {
                ReleaseInfo? update = await updates.CheckForUpdates();
                updates.ShowNotification(update);
            })
            .AddTo(helpMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderAbout, Symbol.Info)
            .WithCommand(windowDialogs.ShowAboutDialogAsync)
            .AddTo(helpMenu);

        return helpMenu;
    }
}