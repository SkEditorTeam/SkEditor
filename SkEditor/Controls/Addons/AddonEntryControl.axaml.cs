using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;
using SkEditor.API;
using SkEditor.Utilities.InternalAPI;
using SkEditor.Views.Settings;

namespace SkEditor.Controls.Addons;

public partial class AddonEntryControl : UserControl
{
    private AddonsPage _addonsPage = null!;
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
        DeleteButton.Command = new RelayCommand(() =>
        {
            AddonLoader.DeleteAddon(addonMeta.Addon);
            _addonsPage.LoadAddons();
        });

        var enabled = addonMeta.State == IAddons.AddonState.Enabled;
        SetStateButton(enabled);
        
        StateButton.IsEnabled = !addonMeta.HasCriticalErrors;
        StateButton.Click += async (_, _) =>
        {
            StateButton.IsEnabled = false;
            var enabled = addonMeta.State == IAddons.AddonState.Enabled;
            if (enabled)
            {
                SkEditorAPI.Addons.DisableAddon(addonMeta.Addon);
                SetStateButton(false);
            }
            else
            {
                var success = await SkEditorAPI.Addons.EnableAddon(addonMeta.Addon);
                SetStateButton(success);
            }
            
            StateButton.IsEnabled = !addonMeta.HasCriticalErrors;
            LoadVisuals(addonMeta);
        };

        if (addonMeta.NeedsRestart) {
            StateButton.IsEnabled = false;
            StateButton.Content = "Restart Required";
        }
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
    
    private static readonly Color ErrorColor = Colors.OrangeRed;
    private static readonly Color WarningColor = Colors.Orange;

    public void LoadVisuals(AddonMeta addonMeta)
    {
        var addon = addonMeta.Addon;
        Expander.Header = addon.Name;
        Expander.Description = addon.Description;
        Expander.IconSource = addon.GetAddonIcon();
        
        if (addonMeta.HasErrors)
        {
            Expander.IconSource = new SymbolIconSource()
            {
                Symbol = Symbol.Warning,
                Foreground = new SolidColorBrush(addonMeta.HasCriticalErrors ? ErrorColor : WarningColor),
                FontSize = 36,
                IsFilled = true
            };
            Expander.Header = new TextBlock()
            {
                Text = addon.Name,
                Foreground = new SolidColorBrush(addonMeta.HasCriticalErrors ? ErrorColor : WarningColor),
                TextDecorations = TextDecorations.Strikethrough,
            };
            
            var panels = new StackPanel()
            {
                Spacing = 2,
            };
            foreach (var error in addonMeta.Errors)
            {
                var textBlock = new TextBlock()
                {
                    Text = "• " + error.Message,
                    Foreground = new SolidColorBrush(error.IsCritical ? ErrorColor : WarningColor),
                    TextWrapping = TextWrapping.Wrap
                };
                panels.Children.Add(textBlock);
            }
            Expander.Items.Add(panels);
        }
        
        if (addonMeta.DllFilePath == null)
            ControlsPanel.IsVisible = false;
        
        if (addonMeta.NeedsRestart)
        {
            var restartText = new TextBlock()
            {
                Text = "This addon requires a restart to take effect.",
                Foreground = new SolidColorBrush(Colors.Gray),
                TextWrapping = TextWrapping.Wrap
            };
            Expander.Items.Add(restartText);
        }
    }
}