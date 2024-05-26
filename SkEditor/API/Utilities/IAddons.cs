using System.Collections.Generic;

namespace SkEditor.API;

/// <summary>
/// Interface for Addons.
/// </summary>
public interface IAddons
{
    
    /// <summary>
    /// Get the <see cref="AddonState"/> of an addon.
    /// </summary>
    /// <param name="addon">The addon to get the state of.</param>
    /// <returns>The state of the addon.</returns>
    public AddonState GetAddonState(IAddon addon);
    
    /// <summary>
    /// Enables the specified addon.
    /// </summary>
    /// <param name="addon">The addon to enable.</param>
    /// <returns>True if the addon was enabled successfully, false otherwise.</returns>
    public bool EnableAddon(IAddon addon);

    /// <summary>
    /// Disables the specified addon.
    /// </summary>
    /// <param name="addon">The addon to disable.</param>
    public void DisableAddon(IAddon addon);

    /// <summary>
    /// Retrieves the addon with the specified name.
    /// </summary>
    /// <param name="addonName">The name of the addon to retrieve.</param>
    /// <returns>The addon with the specified name, or null if no such addon exists.</returns>
    public IAddon? GetAddon(string addonName);

    /// <summary>
    /// Retrieves all addons with the specified state.
    /// </summary>
    /// <param name="state">The state of the addons to retrieve. Defaults to AddonState.Installed.</param>
    /// <returns>An enumerable of addons with the specified state.</returns>
    public IEnumerable<IAddon> GetAddons(AddonState state = AddonState.Installed);
    
    /// <summary>
    /// Get the self addon of SkEditor. This is the addon that represents SkEditor itself.
    /// </summary>
    /// <returns>The self addon of SkEditor.</returns>
    public SkEditorSelfAddon GetSelfAddon();
    
    /// <summary>
    /// Represents the state of an addon.
    /// </summary>
    public enum AddonState
    {
        /// <summary>
        /// The addon is installed. This regroups both enabled and disabled addons.
        /// </summary>
        Installed,
        
        /// <summary>
        /// The addon is enabled.
        /// </summary>
        Enabled,
        
        /// <summary>
        /// The addon is disabled.
        /// </summary>
        Disabled
    }
}