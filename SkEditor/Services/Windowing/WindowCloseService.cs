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
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Services.Windowing;

public class WindowCloseService(ITabService tabs, IDialogService dialogs, IConfigService configs) : IWindowCloseService
{
    public bool IsClosing { get; private set; }

    public async Task<bool> OnWindowClosing(double height, double width, double x, double y, WindowState windowState)
    {
        if (IsClosing)
        {
            return true;
        }

        List<FileTabViewModel> unsavedTabs = tabs.Tabs
            .OfType<FileTabViewModel>()
            .Where(t => !t.IsSaved)
            .ToList();

        if (unsavedTabs.Count > 0)
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

            if (result == DialogResult.Yes)
            {
                bool allSaved = true;
                foreach (FileTabViewModel tab in unsavedTabs)
                {
                    bool saved = await tab.SaveAsync();
                    if (saved)
                    {
                        continue;
                    }

                    allSaved = false;
                    break;
                }

                if (!allSaved)
                {
                    IsClosing = false;
                    return false;
                }
            }
        }

        IsClosing = true;
        await SaveWindowStateAsync(height, width, (int)x, (int)y, windowState);
        return true;
    }

    private async Task SaveWindowStateAsync(double height, double width, int x, int y, WindowState windowState)
    {
        StateConfig? stateConfig = configs.GetConfig<StateConfig>();

        stateConfig?.MainWindowHeight = height;
        stateConfig?.MainWindowWidth = width;
        stateConfig?.MainWindowX = x;
        stateConfig?.MainWindowY = y;
        stateConfig?.MainWindowMaximized = windowState == WindowState.Maximized;

        await configs.SaveAllConfigsImmediateAsync();
    }
}