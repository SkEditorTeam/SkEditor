using System.Collections.Generic;

namespace SkEditor.API;

public class Addons : IAddons
{
    public IAddons.AddonState GetAddonState(IAddon addon)
    {
        if (AddonLoader.EnabledAddons.Contains(addon))
            return IAddons.AddonState.Enabled;
        if (AddonLoader.DisabledAddons.Contains(addon))
            return IAddons.AddonState.Disabled;
        
        return IAddons.AddonState.Installed;
    }

    public void EnableAddon(IAddon addon)
    {
        var state = GetAddonState(addon);
        if (state == IAddons.AddonState.Enabled)
            return;

        if (addon is SkEditorSelfAddon)
        {
            (SkEditorAPI.Logs as Logs).AddonError("Cannot enable the self addon of SkEditor.", true);
            return;
        }
        
        AddonLoader.EnableAddon(addon);
    }

    public void DisableAddon(IAddon addon)
    {
        if (GetAddonState(addon) == IAddons.AddonState.Disabled)
            return;
        
        if (addon is SkEditorSelfAddon)
        {
            (SkEditorAPI.Logs as Logs).AddonError("Cannot disable the self addon of SkEditor.", true);
            return;
        }
        
        AddonLoader.DisableAddon(addon);
    }

    public IAddon? GetAddon(string addonName)
    {
        return AddonLoader.AllAddons.Find(addon => addon.Name == addonName);
    }

    public IEnumerable<IAddon> GetAddons(IAddons.AddonState state = IAddons.AddonState.Installed)
    {
        return state switch
        {
            IAddons.AddonState.Enabled => AddonLoader.EnabledAddons,
            IAddons.AddonState.Disabled => AddonLoader.DisabledAddons,
            _ => AddonLoader.AllAddons
        };
    }
    
    public SkEditorSelfAddon GetSelfAddon()
    {
        return AddonLoader.GetCoreAddon();
    }
}