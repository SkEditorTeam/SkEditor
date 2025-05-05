using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;
using SkEditor.API;
using SkEditor.Utilities.InternalAPI;
using SkEditor.Views;
using SkEditor.Views.Settings;

namespace SkEditor.Controls.Addons;

public partial class AddonEntryControl : UserControl
{
    private static readonly Color ErrorColor = Colors.OrangeRed;
    private static readonly Color WarningColor = Colors.Orange;
    private readonly AddonsPage _addonsPage;

    public AddonEntryControl(AddonMeta addonMeta, AddonsPage addonsPage)
    {
        InitializeComponent();
        _addonsPage = addonsPage;
        addonMeta = AddonLoader.Addons.First(x => x.Addon.Identifier == addonMeta.Addon.Identifier);

        LoadVisuals(addonMeta);
        AssignCommands(addonMeta);
    }

    public void AssignCommands(AddonMeta addonMeta)
    {
        DeleteButton.Command = new AsyncRelayCommand(async () =>
        {
            await AddonLoader.DeleteAddon(addonMeta.Addon);
            _addonsPage.LoadAddons();
        });

        bool enabled = addonMeta.State == IAddons.AddonState.Enabled;
        SetStateButton(enabled);

        StateButton.IsEnabled = !addonMeta.HasCriticalErrors;
        StateButton.Click += async (_, _) =>
        {
            StateButton.IsEnabled = false;
            bool isAddonEnabled = addonMeta.State == IAddons.AddonState.Enabled;
            if (isAddonEnabled)
            {
                await SkEditorAPI.Addons.DisableAddon(addonMeta.Addon);
                SetStateButton(false);
            }
            else
            {
                bool success = await SkEditorAPI.Addons.EnableAddon(addonMeta.Addon);
                SetStateButton(success);
            }

            StateButton.IsEnabled = !addonMeta.HasCriticalErrors;
            _addonsPage.LoadAddons();
        };

        if (!addonMeta.NeedsRestart)
        {
            return;
        }

        StateButton.IsEnabled = false;
        StateButton.Content = "Restart Required";
    }

    public void SetStateButton(bool enabled)
    {
        if (enabled)
        {
            StateButton.Content = "Disable";
            StateButton.Classes.Remove("accent");
        }
        else
        {
            StateButton.Content = "Enable";
            StateButton.Classes.Add("accent");
        }

        StateButton.IsEnabled = true;
    }

    public void LoadVisuals(AddonMeta addonMeta)
    {
        bool isValid = true;
        IAddon addon = addonMeta.Addon;
        Expander.Header = addon.Name;
        Expander.Description = addon.Description;
        Expander.IconSource = addon.GetAddonIcon();

        if (addonMeta.HasErrors)
        {
            isValid = false;
            Expander.IconSource = new SymbolIconSource
            {
                Symbol = Symbol.Warning,
                Foreground = new SolidColorBrush(addonMeta.HasCriticalErrors ? ErrorColor : WarningColor),
                FontSize = 36,
                IconVariant = IconVariant.Filled
            };
            Expander.Header = new TextBlock
            {
                Text = addon.Name,
                Foreground = new SolidColorBrush(addonMeta.HasCriticalErrors ? ErrorColor : WarningColor),
                TextDecorations = TextDecorations.Strikethrough
            };

            StackPanel panels = new()
            {
                Spacing = 2
            };

            IEnumerable<TextBlock> errorTextBlocks = addonMeta.Errors.Select(error => new TextBlock
            {
                Text = "• " + error.Message,
                Foreground = new SolidColorBrush(error.IsCritical ? ErrorColor : WarningColor),
                TextWrapping = TextWrapping.Wrap
            });

            foreach (TextBlock textBlock in errorTextBlocks)
            {
                panels.Children.Add(textBlock);
            }

            Expander.Items.Add(panels);
        }

        if (addonMeta.DllFilePath == null)
        {
            ControlsPanel.IsVisible = false;
        }

        if (addonMeta.NeedsRestart)
        {
            isValid = false;
            TextBlock restartText = new()
            {
                Text = "This addon requires a restart to take effect.",
                Foreground = new SolidColorBrush(Colors.Gray),
                TextWrapping = TextWrapping.Wrap
            };
            Expander.Items.Add(restartText);
        }

        if (!isValid || addon.GetSettings().Count <= 0
                     || !AddonLoader.IsAddonEnabled(addon))
        {
            return;
        }

        Expander.IsClickEnabled = true;
        Expander.Click += (_, _) =>
        {
            SettingsWindow.NavigateToPage(typeof(CustomAddonSettingsPage));
            CustomAddonSettingsPage.Load(addon);
        };
        Expander.ActionIconSource = new SymbolIconSource
        {
            Symbol = Symbol.Settings
        };
        (Expander.Footer as StackPanel).Margin = new Thickness(0, 0, 5, 0);
    }
}