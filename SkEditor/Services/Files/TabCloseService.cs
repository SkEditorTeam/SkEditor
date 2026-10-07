using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Dialogs;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs.Capabilities;
using TabViewModelBase = SkEditor.ViewModels.Tabs.TabViewModelBase;

namespace SkEditor.Services.Files;

public class TabCloseService(AppSettings appSettings, ITabService tabs, IDialogService dialogs) : ITabCloseService
{
    public async Task CloseMultipleAsync(CloseType closeType)
    {
        List<TabViewModelBase> tabsToClose = GetTabsToClose(closeType);
        if (tabsToClose.Count == 0)
        {
            return;
        }

        List<ISaveableTab> saveableTabs = tabsToClose.OfType<ISaveableTab>().ToList();

        if (appSettings.IsAutoSaveEnabled)
        {
            foreach (ISaveableTab saveableTab in saveableTabs)
            {
                await saveableTab.SaveAsync();
            }
        }

        List<ISaveableTab> unsavedTabs = saveableTabs.Where(t => !t.IsSaved).ToList();

        switch (unsavedTabs.Count)
        {
            case 1:
            {
                TabViewModelBase tab = tabsToClose.First(t => !t.IsSaved);
                tab.CloseCommand?.Execute(null);

                if (tabs.AllTabs.Contains(tab))
                {
                    // user cancelled the close operation
                    return;
                }

                break;
            }
            case > 1:
            {
                bool shouldContinue = await AskForConfirmation(unsavedTabs);
                if (!shouldContinue)
                {
                    return;
                }

                break;
            }
        }

        foreach (TabViewModelBase tab in tabsToClose)
        {
            tabs.CloseTab(tab);
        }
    }

    private async Task<bool> AskForConfirmation(List<ISaveableTab> unsavedTabs)
    {
        Dialog dialog = Dialog
            .Create(TranslationKey.UnsavedFilesCloseAllTitle)
            .WithMessage(TranslationKey.UnsavedFilesCloseAllMessage)
            .WithIcon(Symbol.SaveMultiple)
            .WithButtons(
                new DialogButton(TranslationKey.UnsavedFilesSaveAll, DialogResult.Yes, true),
                new DialogButton(TranslationKey.ButtonDontSave, DialogResult.No),
                DialogButton.Cancel
            );

        DialogResult result = await dialogs.ShowDialogAsync(dialog);

        if (result == StandardDialogResult.Cancel)
        {
            return false;
        }

        if (result == StandardDialogResult.No)
        {
            return true;
        }

        bool savedAll = true;
        foreach (ISaveableTab unsavedTab in unsavedTabs)
        {
            bool saved = await unsavedTab.SaveAsync();
            if (saved)
            {
                continue;
            }

            savedAll = false;
            break;
        }

        return savedAll;
    }

    private List<TabViewModelBase> GetTabsToClose(CloseType closeType)
    {
        return closeType switch
        {
            CloseType.All => tabs.AllTabs.ToList(),
            CloseType.AllButCurrent => tabs.AllTabs.Where(t => t != tabs.SelectedTab).ToList(),
            CloseType.Unsaved => tabs.AllTabs.Where(t => !t.IsSaved).ToList(),
            CloseType.Left => tabs.AllTabs.TakeWhile(t => t != tabs.SelectedTab).ToList(),
            CloseType.Right => tabs.AllTabs.SkipWhile(t => t != tabs.SelectedTab).Skip(1).ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(closeType), closeType, null)
        };
    }
}