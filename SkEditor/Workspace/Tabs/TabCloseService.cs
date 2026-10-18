using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentIcons.Common;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Dialogs.Models;
using SkEditor.Core.Localization;
using SkEditor.Workspace.Tabs.Capabilities;
using SkEditor.Workspace.Tabs.Models;
using AppSettings = SkEditor.Configuration.Configs.AppSettings;

namespace SkEditor.Workspace.Tabs;

public class TabCloseService(AppSettings appSettings, ITabService tabs, IDialogService dialogs) : ITabCloseService
{
    public async Task CloseMultipleAsync(CloseType closeType)
    {
        List<ITab> tabsToClose = GetTabsToClose(closeType);
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
                ITab tab = tabsToClose.First(t => t is ISaveableTab { IsSaved: false });
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

        foreach (ITab tab in tabsToClose)
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

    private List<ITab> GetTabsToClose(CloseType closeType)
    {
        return closeType switch
        {
            CloseType.All => tabs.AllTabs.ToList(),
            CloseType.AllButCurrent => tabs.AllTabs.Where(t => t != tabs.SelectedTab).ToList(),
            CloseType.Unsaved => tabs.AllTabs.Where(t => t is ISaveableTab { IsSaved: false }).ToList(),
            CloseType.Left => tabs.AllTabs.TakeWhile(t => t != tabs.SelectedTab).ToList(),
            CloseType.Right => tabs.AllTabs.SkipWhile(t => t != tabs.SelectedTab).Skip(1).ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(closeType), closeType, null)
        };
    }
}