using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentIcons.Common;
using SkEditor.Configuration;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Dialogs.Models;
using SkEditor.Core.Localization;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Session;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;
using AppSettings = SkEditor.Configuration.Configs.AppSettings;
using StateConfig = SkEditor.Configuration.Configs.StateConfig;

namespace SkEditor.Windowing;

public class WindowCloseService(
    AppSettings appSettings,
    ITabService tabs,
    IDialogService dialogs,
    IConfigService configs,
    ISessionRestoreService sessionRestore) : IWindowCloseService
{
    public bool IsClosing { get; private set; }

    public async Task<bool> OnWindowClosing(double height, double width, double x, double y, WindowState windowState)
    {
        StateConfig? stateConfig = configs.GetConfig<StateConfig>();
        if (IsClosing || stateConfig is null)
        {
            return true;
        }

        List<ISaveableTab> saveableTabs = tabs.AllTabs.OfType<ISaveableTab>().ToList();

        bool isAutoSaveEnabled = appSettings.IsAutoSaveEnabled;
        bool isSessionRestoreEnabled = appSettings.IsSessionRestoreEnabled;

        if (isAutoSaveEnabled)
        {
            foreach (ISaveableTab saveableTab in saveableTabs)
            {
                await saveableTab.SaveAsync();
            }
        }

        List<ISaveableTab> unsavedTabs = saveableTabs
            .Where(t => !t.IsSaved)
            .ToList();

        List<ISaveableTab> remoteTabs = unsavedTabs
            .Where(t => t is IFileTab { File: not LocalFile })
            .ToList();

        bool hasRemoteTabs = remoteTabs.Count > 0;
        if (unsavedTabs.Count > 0 && (!isSessionRestoreEnabled || hasRemoteTabs))
        {
            bool success;
            if (isSessionRestoreEnabled && hasRemoteTabs)
            {
                success = await HandleUnsaved(remoteTabs);
            }
            else
            {
                success = await HandleUnsaved(unsavedTabs);
            }
            
            if (!success)
            {
                return false;
            }
        }

        IsClosing = true;
        await SaveWindowStateAsync(stateConfig, height, width, (int)x, (int)y, windowState);
        await sessionRestore.SaveSessionStateAsync();
        return true;
    }

    private async Task<bool> HandleUnsaved(List<ISaveableTab> unsavedTabs)
    {
        Dialog dialog = Dialog
            .Create(TranslationKey.UnsavedFilesAppCloseTitle)
            .WithMessage(TranslationKey.UnsavedFilesAppCloseMessage)
            .WithIcon(Symbol.Save)
            .WithButtons(
                new DialogButton(TranslationKey.ButtonSave, DialogResult.Yes, true),
                new DialogButton(TranslationKey.ButtonDontSave, DialogResult.No),
                DialogButton.Cancel
            );

        DialogResult result = await dialogs.ShowDialogAsync(dialog);
        if (result == DialogResult.Cancel)
        {
            IsClosing = false;
            return false;
        }

        if (result != DialogResult.Yes)
        {
            return true;
        }

        bool allSaved = true;
        foreach (ISaveableTab tab in unsavedTabs)
        {
            bool saved = await tab.SaveAsync();
            if (saved)
            {
                continue;
            }

            allSaved = false;
            break;
        }

        if (allSaved)
        {
            return true;
        }

        IsClosing = false;
        return false;
    }

    private async Task SaveWindowStateAsync(StateConfig stateConfig, double height, double width, int x, int y,
        WindowState windowState)
    {
        bool isMaximized = windowState == WindowState.Maximized;
        stateConfig?.MainWindowMaximized = isMaximized;
        stateConfig?.MainWindowX = x;
        stateConfig?.MainWindowY = y;

        // TODO: think about saving the last known bounds if the window was unmaximized in this session
        if (!isMaximized)
        {
            stateConfig?.MainWindowHeight = height;
            stateConfig?.MainWindowWidth = width;
        }

        await configs.SaveAllConfigsImmediateAsync();
    }
}