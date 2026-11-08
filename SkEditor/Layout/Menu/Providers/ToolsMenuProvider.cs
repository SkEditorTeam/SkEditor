using System;
using FluentIcons.Common;
using R3;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Localization;
using SkEditor.Documentation.UI;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Layout.Menu.Models;
using SkEditor.Layout.StatusBar.UI;
using SkEditor.Windowing;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Extensions;

namespace SkEditor.Layout.Menu.Providers;

public class ToolsMenuProvider(
    ITabService tabs,
    Func<DocumentationTabViewModel> docsTabFactory,
    BottomDrawerViewModel bottomDrawer,
    ExperimentsConfig experiments,
    IWindowDialogService windowDialogs) : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        BindableReactiveProperty<bool> isTextEditorActive = tabs.ObserveSelectedTab()
            .Select(tab => tab is ITextEditorTab)
            .ToBindableReactiveProperty();
        
        MainMenuItem tools = MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderTools);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderDocumentation, Symbol.Book)
            .WithCommand(() => tabs.OpenUniqueTab(docsTabFactory))
            .AddTo(tools);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderGenerateGUI, Symbol.WindowBrush)
            .WithCommand(windowDialogs.ShowGuiGeneratorDialogAsync)
            .WithEnabled(isTextEditorActive)
            .AddTo(tools);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderTerminal, Symbol.WindowConsole)
            .WithCommand(() => bottomDrawer.TogglePane("Terminal"))
            .WithVisibility(experiments.ObservePropertyChanged(x => x.TerminalEnabled).ToBindableReactiveProperty())
            .AddTo(tools);

        return tools;
    }
}