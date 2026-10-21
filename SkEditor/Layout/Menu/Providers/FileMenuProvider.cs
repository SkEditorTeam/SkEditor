using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using R3;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Layout.Menu.Models;
using SkEditor.Windowing;
using SkEditor.Workspace.Files;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Projects.Messages;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;
using SkEditor.Workspace.Tabs.Extensions;
using SkEditor.Workspace.Tabs.Models;

namespace SkEditor.Layout.Menu.Providers;

public class FileMenuProvider(
    IFileService files,
    ITabService tabs,
    ITabCloseService tabClose,
    IRecentFileService recentFiles,
    ExperimentsConfig experiments,
    IWindowDialogService windowDialogs) : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        Observable<ITab?> selectedTab = tabs.ObserveSelectedTab();
        BindableReactiveProperty<bool> isSaveable =
            selectedTab.Select(tab => tab is ISaveableTab).ToBindableReactiveProperty();
        BindableReactiveProperty<bool> hasTabs =
            selectedTab.Select(_ => tabs.AllTabs.Any()).ToBindableReactiveProperty();

        MainMenuItem fileMenu = MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderFile);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderNew, Symbol.Document)
            .WithCommand(() =>
            {
                ITab tab = files.NewFile();
                tabs.OpenTab(tab);
            })
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

        MainMenuItem historyItem = BuildHistoryItem();
        fileMenu.WithItem(historyItem);

        MainMenuItem.Separator.AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSave, Symbol.Save)
            .WithCommand(async () =>
            {
                if (tabs.SelectedTab is IFileTab fileTab)
                {
                    await files.SaveFileAsync(fileTab);
                }
            })
            .WithInputGesture("Ctrl+S")
            .WithEnabled(isSaveable)
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAs, Symbol.SaveEdit)
            .WithCommand(async () =>
            {
                if (tabs.SelectedTab is IFileTab fileTab)
                {
                    await files.SaveFileAsAsync(fileTab);
                }
            })
            .WithInputGesture("Ctrl+Shift+S")
            .WithEnabled(isSaveable)
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAll, Symbol.SaveMultiple)
            .WithCommand(async () =>
            {
                foreach (IFileTab fileTab in tabs.AllTabs.OfType<IFileTab>())
                {
                    await files.SaveFileAsync(fileTab);
                }
            })
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

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSplitTabs, Symbol.DocumentLandscapeSplitHint)
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
                    .WithInputGesture("Ctrl+W")
                    .WithCommand(async () =>
                    {
                        if (tabs.SelectedTab is not null)
                        {
                            await tabs.CloseTabAsync(tabs.SelectedTab);
                        }
                    }),

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

    private MainMenuItem BuildHistoryItem()
    {
        MainMenuItem historyItem = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderRecentFiles, Symbol.History);
        ObservableCollection<MainMenuItem> recentItems = [];
        historyItem.WithItems(recentItems);

        recentFiles.RecentItems.BindTo(recentItems, item =>
        {
            return MainMenuItem
                .HeaderWithIcon(item.Name.ToTranslatable(), Symbol.Document)
                .WithTooltip(item.Path.ToTranslatable())
                .WithCommand(() =>
                {
                    Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await files.OpenFileAsync(new LocalFile(item.Path));
                    });
                });
        }, i => !i.IsFolder, 5);

        return historyItem;
    }
}