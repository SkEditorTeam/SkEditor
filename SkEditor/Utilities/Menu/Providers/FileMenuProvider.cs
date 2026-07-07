using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using R3;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Messages;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings;
using SkEditor.ViewModels.Tabs;
using SkEditor.ViewModels.Tabs.Capabilities;
using MainMenuItem = SkEditor.ViewModels.Menu.MainMenuItem;

namespace SkEditor.Utilities.Menu.Providers;

public class FileMenuProvider(
    IFileService files,
    ITabService tabs,
    ITabCloseService tabClose,
    ExperimentsConfig experiments,
    IWindowDialogService windowDialogs) : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        Observable<TabViewModelBase?> selectedTab = tabs.ObserveSelectedTab();
        BindableReactiveProperty<bool> isSaveable =
            selectedTab.Select(tab => tab is ISaveableTab).ToBindableReactiveProperty();
        BindableReactiveProperty<bool> hasTabs =
            selectedTab.Select(_ => tabs.AllTabs.Any()).ToBindableReactiveProperty();

        MainMenuItem fileMenu = MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderFile);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderNew, Symbol.Document)
            .WithCommand(files.NewFile)
            .WithInputGesture("Ctrl+N")
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderOpen, Symbol.DocumentSearch)
            .WithCommand(files.OpenFileAsync)
            .WithInputGesture("Ctrl+O")
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderOpenFolder, Symbol.FolderOpen)
            .WithCommand(() => WeakReferenceMessenger.Default.Send(new OpenExplorerFolderMessage()))
            .WithInputGesture("Ctrl+Shift+O")
            .AddTo(fileMenu);

        MainMenuItem.Separator.AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSave, Symbol.Save)
            .WithCommand(files.SaveFileAsync)
            .WithInputGesture("Ctrl+S")
            .WithEnabled(isSaveable)
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAs, Symbol.SaveEdit)
            .WithCommand(files.SaveAsFileAsync)
            .WithInputGesture("Ctrl+Shift+S")
            .WithEnabled(isSaveable)
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAll, Symbol.SaveMultiple)
            .WithCommand(files.SaveAllAsync)
            .WithInputGesture("Ctrl+Alt+S")
            .WithEnabled(selectedTab.Select(_ => tabs.AllTabs.Any() && tabs.AllTabs.Any(t => t is ISaveableTab))
                .ToBindableReactiveProperty())
            .AddTo(fileMenu);

        MainMenuItem.Separator.AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderPublish, Symbol.Share)
            .WithCommand(windowDialogs.ShowPublishDialogAsync)
            .WithInputGesture("Ctrl+Shift+P")
            .WithEnabled(selectedTab.Select(tab => tab is ITextEditorTab).ToBindableReactiveProperty())
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslatableString.FromRawString("Split tabs"), Symbol.DocumentLandscapeSplitHint)
            .WithCommand(() =>
            {
                if (tabs.SelectedTab is not null)
                {
                    tabs.SplitTab(tabs.SelectedTab);
                }
            })
            .WithEnabled(selectedTab.Select(_ => tabs.SelectedTab is not null && tabs.AllTabs.Count() > 1)
                .ToBindableReactiveProperty())
            .WithVisibility(experiments.ObservePropertyChanged(x => x.SplitViewEnabled).ToBindableReactiveProperty())
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderClose, Symbol.Dismiss)
            .WithEnabled(hasTabs)
            .WithItems([
                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseCurrentFile, Symbol.Document)
                    .WithCommand(() => tabs.SelectedTab?.CloseCommand.Execute(null)),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseAll, Symbol.DocumentMultiple)
                    .WithCommand(() => tabClose.CloseMultipleAsync(CloseType.All)),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseAllButCurrent, Symbol.DocumentMultiple)
                    .WithCommand(() => tabClose.CloseMultipleAsync(CloseType.AllButCurrent)),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseUnsaved, Symbol.DocumentMultiple)
                    .WithCommand(() => tabClose.CloseMultipleAsync(CloseType.Unsaved)),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseLeft, Symbol.DocumentArrowLeft)
                    .WithCommand(() => tabClose.CloseMultipleAsync(CloseType.Left)),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseRight, Symbol.DocumentArrowRight)
                    .WithCommand(() => tabClose.CloseMultipleAsync(CloseType.Right))
            ])
            .AddTo(fileMenu);

        return fileMenu;
    }
}