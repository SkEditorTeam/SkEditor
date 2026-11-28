using System;
using System.Collections.Generic;
using FluentIcons.Common;
using SkEditor.Configuration.Configs;
using SkEditor.Configuration.UI;
using SkEditor.Welcome.Models;
using SkEditor.Workspace.Tabs;

namespace SkEditor.Welcome;

public sealed class WhatsNewService(StateConfig state, ITabService tabs,  Func<WhatsNewPageViewModel> whatsNewFactory) : IWhatsNewService
{
    public void ShowWhatsNew()
    {
        if (!state.ShouldShowWhatsNewPage)
        {
            return;
        }
        
        tabs.OpenUniqueTab(whatsNewFactory);
    }

    public IReadOnlyList<WhatsNewFeaturedItem> GetFeaturedItems()
    {
        return
        [
            new WhatsNewFeaturedItem(
                Symbol.Flash,
                "Natively compiled",
                "Instant startup, lower memory usage, and a much better architecture built on the latest .NET"),

            new WhatsNewFeaturedItem(
                "RemoteDesktopIcon",
                "SFTP Remote Explorer",
                "Connect directly to your remote server, browse files, and edit scripts without external software"),

            new WhatsNewFeaturedItem(
                Symbol.History,
                "Session restore you can trust",
                "Session restore is now fully reliable, so you can pick up where you left off without worry"),

            new WhatsNewFeaturedItem(
                Symbol.Book,
                "Redesigned documentation",
                "Browse SkDocs and skUnity syntaxes blazingly fast without leaving the editor"),

            new WhatsNewFeaturedItem(
                Symbol.CodeTextEdit,
                "New syntax highlighting",
                "Full TextMate grammar and theme support for Skript and dozens of other file formats"),

            new WhatsNewFeaturedItem(
                Symbol.DocumentSearch,
                "The new Analyzer",
                "Check your scripts for errors and warnings with the new Analyzer, now built directly into SkEditor")
        ];
    }
}