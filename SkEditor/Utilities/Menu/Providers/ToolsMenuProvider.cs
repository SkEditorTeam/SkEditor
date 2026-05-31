using System;
using FluentIcons.Common;
using SkEditor.Contracts.Services.Files;
using SkEditor.Utilities.Localization;
using SkEditor.ViewModels.Tabs;
using MainMenuItem = SkEditor.ViewModels.Menu.MainMenuItem;

namespace SkEditor.Utilities.Menu.Providers;

public class ToolsMenuProvider(ITabService tabs, Func<DocumentationTabViewModel> docsTabFactory) : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        MainMenuItem tools = MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderTools);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderDocumentation, Symbol.Book)
            .WithCommand(() => tabs.OpenUniqueTab(docsTabFactory))
            .AddTo(tools);

        return tools;
    }
}