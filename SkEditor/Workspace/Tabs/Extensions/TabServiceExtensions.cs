using System;
using System.IO;
using R3;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Workspace.Tabs.Extensions;

public static class TabServiceExtensions
{
    public static Observable<ITab?> ObserveSelectedTab(this ITabService tabService)
    {
        return Observable.FromEvent<Action<ITab?>, ITab?>(
                h => h,
                h => tabService.SelectedTabChanged += h,
                h => tabService.SelectedTabChanged -= h)
            .Prepend(tabService.SelectedTab);
    }

    extension(IFileTab tab)
    {
        public bool IsAnalyzable
        {
            get
            {
                string extension = Path.GetExtension(tab.Path ?? string.Empty).ToLowerInvariant();
                return string.IsNullOrWhiteSpace(extension) || extension is ".sk";
            }
        }
    }
}