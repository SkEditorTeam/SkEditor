using System;
using R3;
using SkEditor.Contracts.Services.Files;
using SkEditor.ViewModels.Tabs;

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
}