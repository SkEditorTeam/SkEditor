using SkEditor.Utilities.InternalAPI;

namespace SkEditor.API;

public static class AddonExtensions
{
    public static object? GetSetting(this IAddon addon, string key)
    {
        return AddonSettingsManager.GetAddonValue(addon, key);
    }

    public static void SetSetting(this IAddon addon, string key, object value)
    {
        AddonSettingsManager.SetAddonValue(addon, key, value);
    }
}