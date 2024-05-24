using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SkEditor.API;

namespace SkEditor.Controls.Addons;

public partial class AddonEntryControl : UserControl
{
    public AddonEntryControl(IAddon addon)
    {
        InitializeComponent();

        AssignCommands(addon);
        LoadVisuals(addon);
    }

    public void AssignCommands(IAddon addon)
    {
        var enabled = AddonLoader.IsAddonEnabled(addon);
        SetStateButton(enabled);
        
        StateButton.Click += (_, _) =>
        {
            var enabled = AddonLoader.IsAddonEnabled(addon);
            if (enabled)
            {
                AddonLoader.DisableAddon(addon);
                SetStateButton(false);
            }
            else
            {
                AddonLoader.EnableAddon(addon);
                SetStateButton(true);
            }
        };
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
    }

    public void LoadVisuals(IAddon addon)
    {
        Expander.Header = addon.Name;
        Expander.Description = addon.Description;
        Expander.IconSource = addon.GetAddonIcon();
        
        // TODO: Invert the condition so it not does show SkEditorCor's buttons
        if (!addon.GetType().Namespace.StartsWith("SkEditor"))
        {
            ControlsPanel.IsVisible = false;
        }
    }
}