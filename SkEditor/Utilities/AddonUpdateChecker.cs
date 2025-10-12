using System.Collections.Generic;
using System.Threading.Tasks;
using SkEditor.API;
using SkEditor.Views.Windows.Marketplace;
using SkEditor.Views.Windows.Marketplace.Types;
using Symbol = FluentIcons.Common.Symbol;

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
                if (item is not AddonItem addonItem)
                {
                    continue;
                }

                string? currentVersion = addonItem.GetAddon()?.Version;
                string? latestVersion = item.ItemVersion;

                if (currentVersion == null || latestVersion == null)
                {
                    continue;
                }

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