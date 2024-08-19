using System;

namespace SkEditor.API;

/// <summary>
/// Object representing a registry key, based on an IAddon instance and a string key.
/// </summary>
/// <param name="Addon">The IAddon instance linked to the key.</param>
/// <param name="Key">The string key.</param>
public record RegistryKey(IAddon Addon, string Key)
{

    /// <summary>
    /// Get the full key, which is the addon identifier followed by a slash and the key.
    /// </summary>
    public string FullKey => $"{Addon.Identifier}/{Key}";

    /// <summary>
    /// Create a new instance of a registry key from a (text) full key.
    /// </summary>
    /// <param name="fullKey">The full key to create the instance from.</param>
    /// <returns>A new instance of a registry key.</returns>
    /// <exception cref="ArgumentException">Thrown if the full key is not in the correct format, i.e. does not contain a slash.</exception>
    public static RegistryKey FromFullKey(string fullKey)
    {
        var parts = fullKey.Split('/');
        if (parts.Length != 2)
            throw new ArgumentException("Invalid full key format.");

        return new RegistryKey(SkEditorAPI.Addons.GetAddon(parts[0]), parts[1]);
    }
};