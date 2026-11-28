using System;
using SkEditor.Configuration.Configs;
using SkEditor.Configuration.UI;
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
}