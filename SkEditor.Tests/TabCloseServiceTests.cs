using NSubstitute;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Dialogs.Models;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;
using SkEditor.Workspace.Tabs.Models;
using AppSettings = SkEditor.Configuration.Configs.AppSettings;

namespace SkEditor.Tests;

public class TabCloseServiceTests
{
    [Fact]
    public async Task CloseMultipleAsync_WhenUserCancelsDialog_DoesNotCloseAnyTabs()
    {
        AppSettings appSettings = new() { IsAutoSaveEnabled = false };
        ITabService tabs = Substitute.For<ITabService>();
        IDialogService dialogs = Substitute.For<IDialogService>();

        ITab tab1 = Substitute.For<ITab, ISaveableTab>();
        ((ISaveableTab)tab1).IsSaved.Returns(false);

        ITab tab2 = Substitute.For<ITab, ISaveableTab>();
        ((ISaveableTab)tab2).IsSaved.Returns(false);

        List<ITab> tabList = [tab1, tab2];
        tabs.AllTabs.Returns(tabList);

        dialogs.ShowDialogAsync(Arg.Any<Dialog>()).Returns(DialogResult.Cancel);

        TabCloseService service = new(appSettings, tabs, dialogs);

        await service.CloseMultipleAsync(CloseType.All);

        await dialogs.Received(1).ShowDialogAsync(Arg.Any<Dialog>());

        tabs.DidNotReceive().ForceCloseTab(Arg.Any<ITab>());
    }
}