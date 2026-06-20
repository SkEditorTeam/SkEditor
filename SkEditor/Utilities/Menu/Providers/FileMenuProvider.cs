using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Messages;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings;
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

        MainMenuItem saveItem = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSave, Symbol.Save)
            .WithCommand(files.SaveFileAsync)
            .WithInputGesture("Ctrl+S");
        saveItem.AddTo(fileMenu);

        MainMenuItem saveAsItem = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAs, Symbol.SaveEdit)
            .WithCommand(files.SaveAsFileAsync)
            .WithInputGesture("Ctrl+Shift+S");
        saveAsItem.AddTo(fileMenu);

        MainMenuItem saveAllItem = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAll, Symbol.SaveMultiple)
            .WithCommand(files.SaveAllAsync)
            .WithInputGesture("Ctrl+Alt+S");
        saveAllItem.AddTo(fileMenu);

        MainMenuItem.Separator.AddTo(fileMenu);

        MainMenuItem publishItem = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderPublish, Symbol.Share)
            .WithCommand(windowDialogs.ShowPublishDialogAsync)
            .WithInputGesture("Ctrl+Shift+P");
        publishItem.AddTo(fileMenu);

        MainMenuItem splitTabsItem = MainMenuItem
            .HeaderWithIcon(TranslatableString.FromRawString("Split tabs"), Symbol.DocumentLandscapeSplitHint)
            .WithCommand(() =>
            {
                if (tabs.SelectedTab is not null)
                {
                    tabs.SplitTab(tabs.SelectedTab);
                }
            });
        splitTabsItem.AddTo(fileMenu);

        MainMenuItem closeItem = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderClose, Symbol.Dismiss)
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
            ]);
        closeItem.AddTo(fileMenu);

        UpdateStates(saveItem, saveAsItem, saveAllItem, publishItem, closeItem, splitTabsItem);
        tabs.SelectedTabChanged += _ =>
            UpdateStates(saveItem, saveAsItem, saveAllItem, publishItem, closeItem, splitTabsItem);

        splitTabsItem.IsVisible = experiments.SplitViewEnabled;
        experiments.PropertyChanged += (_, _) => splitTabsItem.IsVisible = experiments.SplitViewEnabled;

        return fileMenu;
    }

    private void UpdateStates(MainMenuItem saveItem, MainMenuItem saveAsItem, MainMenuItem saveAllItem,
        MainMenuItem publishItem, MainMenuItem closeItem, MainMenuItem splitTabsItem)
    {
        bool hasTabs = tabs.AllTabs.Any();
        bool isSaveableTab = tabs.SelectedTab is ISaveableTab;
        bool isTextEditorTab = tabs.SelectedTab is ITextEditorTab;

        saveItem.IsEnabled = isSaveableTab;
        saveAsItem.IsEnabled = isSaveableTab;

        saveAllItem.IsEnabled = hasTabs && tabs.AllTabs.Any(t => t is ISaveableTab);

        publishItem.IsEnabled = isTextEditorTab;

        closeItem.IsEnabled = hasTabs;

        splitTabsItem.IsEnabled = tabs.SelectedTab is not null && tabs.AllTabs.Count() > 1;
    }
}