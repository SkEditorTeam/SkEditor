using System;
using FluentIcons.Common;
using R3;
using SkEditor.Core.Localization;
using SkEditor.Layout.StatusBar.UI;
using SkEditor.Workspace.Tabs;
using DocumentationTabViewModel = SkEditor.Documentation.UI.DocumentationTabViewModel;
using ExperimentsConfig = SkEditor.Configuration.Configs.ExperimentsConfig;
using MainMenuItem = SkEditor.Layout.Menu.Models.MainMenuItem;

namespace SkEditor.Layout.Menu.Providers;

public class ToolsMenuProvider(
    ITabService tabs,
    Func<DocumentationTabViewModel> docsTabFactory,
    BottomDrawerViewModel bottomDrawer,
    ExperimentsConfig experiments) : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        MainMenuItem tools = MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderTools);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderDocumentation, Symbol.Book)
            .WithCommand(() => tabs.OpenUniqueTab(docsTabFactory))
            .AddTo(tools);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderTerminal, Symbol.WindowConsole)
            .WithCommand(() => bottomDrawer.TogglePane("Terminal"))
            .WithVisibility(experiments.ObservePropertyChanged(x => x.TerminalEnabled).ToBindableReactiveProperty())
            .AddTo(tools);

        return tools;
    }
}