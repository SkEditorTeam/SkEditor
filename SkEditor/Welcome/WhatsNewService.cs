using System;
using System.Collections.Generic;
using FluentIcons.Common;
using SkEditor.Configuration.Configs;
using SkEditor.Configuration.UI;
using SkEditor.Core.Localization;
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
                new TranslatableString(TranslationKey.WhatsNewNativeTitle),
                new TranslatableString(TranslationKey.WhatsNewNativeDescription)),

            new WhatsNewFeaturedItem(
                "RemoteDesktopIcon",
                new TranslatableString(TranslationKey.WhatsNewRemoteTitle),
                new TranslatableString(TranslationKey.WhatsNewRemoteDescription)),

            new WhatsNewFeaturedItem(
                Symbol.History,
                new TranslatableString(TranslationKey.WhatsNewSessionTitle),
                new TranslatableString(TranslationKey.WhatsNewSessionDescription)),

            new WhatsNewFeaturedItem(
                Symbol.Book,
                new TranslatableString(TranslationKey.WhatsNewDocumentationTitle),
                new TranslatableString(TranslationKey.WhatsNewDocumentationDescription)),

            new WhatsNewFeaturedItem(
                Symbol.CodeTextEdit,
                new TranslatableString(TranslationKey.WhatsNewHighlightingTitle),
                new TranslatableString(TranslationKey.WhatsNewHighlightingDescription)),

            new WhatsNewFeaturedItem(
                Symbol.DocumentSearch,
                new TranslatableString(TranslationKey.WhatsNewAnalyzerTitle),
                new TranslatableString(TranslationKey.WhatsNewAnalyzerDescription))
        ];
    }
}