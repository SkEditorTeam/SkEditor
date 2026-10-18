using System;
using FluentIcons.Common;
using R3;
using SkEditor.Contracts.Services.Files;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Tabs;
using MainMenuItem = SkEditor.ViewModels.Menu.MainMenuItem;

namespace SkEditor.Utilities.Menu.Providers;

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