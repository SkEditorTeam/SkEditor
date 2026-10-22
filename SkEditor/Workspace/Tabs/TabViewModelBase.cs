using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using SkEditor.Core.Localization;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Workspace.Tabs;

/// <summary>
///     Represents a tab in the app. This is an abstract class that should be inherited by specific types of tabs (file
///     tabs, settings tabs, etc.).
/// </summary>
public abstract partial class TabViewModelBase : ObservableObject, ITab
{
    /// <summary>
    ///     Represents a tab in the app. This is an abstract class that should be inherited by specific types of tabs (file
    ///     tabs, settings tabs, etc.).
    /// </summary>
    /// <param name="header">The header text of the tab, which is displayed on the tab itself.</param>
    /// <param name="icon">The Symbol icon that is displayed on the tab.</param>
    protected TabViewModelBase(TranslatableString header, Symbol? icon = null)
    {
        Header = header;
        Icon = icon;
    }

    /// <summary>
    ///     The header text of the tab, which is displayed on the tab itself.
    /// </summary>
    [ObservableProperty]
    public partial TranslatableString Header { get; protected set; }

    [ObservableProperty]
    public virtual partial string? ToolTip { get; set; }

    /// <summary>
    ///     The Symbol icon that is displayed on the tab.
    /// </summary>
    [ObservableProperty]
    public partial Symbol? Icon { get; protected set; }

    /// <summary>
    ///     Indicates whether the tab's content should be cached when switching tabs.
    ///     If true, the content will not get recreated when the user switches back to the tab.
    /// </summary>
    public virtual bool IsCacheable => false;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = false;

    /// <summary>
    ///     Indicates whether the content of the tab has unsaved changes.
    /// </summary>
    [ObservableProperty]
    public partial bool IsSaved { get; set; } = true;
}