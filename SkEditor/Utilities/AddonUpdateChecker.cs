using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Views.Marketplace;
using SkEditor.Views.Marketplace.Types;

namespace SkEditor.Utilities;

public static class AddonUpdateChecker
{
    public static async Task CheckForUpdates()
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
}