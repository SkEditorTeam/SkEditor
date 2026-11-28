using FluentIcons.Common;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Extensions;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Configuration.UI;

public class WhatsNewPageViewModel(StateConfig state) : TabViewModelBase("What's new".ToTranslatableWip(), Symbol.Sparkle), IClosableTab
{
    public void OnClosed()
    {
        state.ShouldShowWhatsNewPage = false;
    }
}