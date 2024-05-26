using System.Collections.Generic;
using System.Linq;
using SkEditor.Utilities.InternalAPI;

namespace SkEditor.API;

public class Addons : IAddons
{
    public IAddons.AddonState GetAddonState(IAddon addon)
    {
        return AddonLoader.GetAddonState(addon);
    }

    public bool EnableAddon(IAddon addon)
    {
        var state = GetAddonState(addon);
        if (state == IAddons.AddonState.Enabled)
            return true;

        if (addon is SkEditorSelfAddon)
        {
            (SkEditorAPI.Logs as Logs).AddonError("Cannot enable the self addon of SkEditor.", true);
            return false;
        }
        
        return AddonLoader.EnableAddon(addon);
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

    public IAddon? GetAddon(string addonIdentifier)
    {
        return AddonLoader.Addons.FirstOrDefault(a => a.Addon.Identifier == addonIdentifier)?.Addon;
    }

    public IEnumerable<IAddon> GetAddons(IAddons.AddonState state = IAddons.AddonState.Installed)
    {
        return state == IAddons.AddonState.Installed 
            ? AddonLoader.Addons.Select(a => a.Addon) 
            : AddonLoader.Addons.Where(a => GetAddonState(a.Addon) == state).Select(a => a.Addon);
    }
    
    public SkEditorSelfAddon GetSelfAddon()
    {
        return AddonLoader.GetCoreAddon();
    }
}