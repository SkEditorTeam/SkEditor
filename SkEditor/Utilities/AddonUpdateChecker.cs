using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Views.Windows.Marketplace;
using SkEditor.Views.Windows.Marketplace.Types;

namespace SkEditor.Utilities;

public static class AddonUpdateChecker
{
    public static async Task CheckForUpdates()
    {
        try
        {
            IAsyncEnumerable<MarketplaceItem> items = MarketplaceLoader.GetItems();

            await foreach (MarketplaceItem item in items)
            {
                if (item is not AddonItem addonItem) continue;

                var currentVersion = addonItem.GetAddon()?.Version;
                var latestVersion = item.ItemVersion;

                if (currentVersion == null || latestVersion == null) continue;

                if (currentVersion != latestVersion)
                {
                    await SkEditorAPI.Windows.ShowDialog(Translation.Get("AddonUpdateAvailableTitle"),
                        Translation.Get("AddonUpdateAvailableMessage", addonItem.ItemName),
                        Symbol.AlertUrgent);
                }
            }
        }
        catch
        {
            // ignore - we won't bother the user, if he has no internet connection or something else goes wrong
        }
    }
}