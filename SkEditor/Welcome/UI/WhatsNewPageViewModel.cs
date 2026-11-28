using System.Collections.Generic;
using FluentIcons.Common;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Localization;
using SkEditor.Welcome;
using SkEditor.Welcome.Models;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Configuration.UI;

public class WhatsNewPageViewModel(StateConfig state, IWhatsNewService whatsNew)
    : TabViewModelBase(TranslationKey.WhatsNewPageTitle, Symbol.Sparkle), IClosableTab
{
    public IReadOnlyList<WhatsNewFeaturedItem> FeaturedItems { get; } = whatsNew.GetFeaturedItems();

    public void OnClosed()
    {
        state.ShouldShowWhatsNewPage = false;
    }
}