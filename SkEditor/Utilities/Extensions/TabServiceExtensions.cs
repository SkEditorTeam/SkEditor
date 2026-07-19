using System;
using System.IO;
using R3;
using SkEditor.Contracts.Services.Files;
using SkEditor.ViewModels.Tabs;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Utilities.Extensions;

public static class TabServiceExtensions
{
    public static Observable<TabViewModelBase?> ObserveSelectedTab(this ITabService tabService)
    {
        return Observable.FromEvent<Action<TabViewModelBase?>, TabViewModelBase?>(
                h => h,
                h => tabService.SelectedTabChanged += h,
                h => tabService.SelectedTabChanged -= h)
            .Prepend(tabService.SelectedTab);
    }

    extension(IFileTab tab)
    {
        public bool IsAnalyzable => Path.GetExtension(tab.Path ?? string.Empty).ToLowerInvariant() is null or ".sk";
    }
}