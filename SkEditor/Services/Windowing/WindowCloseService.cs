using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Dialogs;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Services.Windowing;

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

        if (unsavedTabs.Count > 0 && !appSettings.IsAutoSaveEnabled)
        {
            bool success = await HandleUnsaved(unsavedTabs);
            if (!success)
            {
                return false;
            }
        }

        IsClosing = true;
        await SaveWindowStateAsync(stateConfig, height, width, (int)x, (int)y, windowState);
        await sessionRestore.SaveSessionState();
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