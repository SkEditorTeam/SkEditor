using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Core.Localization;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Workspace.Tabs;

/// <summary>
///     Represents a tab in the app. This is an abstract class that should be inherited by specific types of tabs (file
///     tabs, settings tabs, etc.).
/// </summary>
/// <param name="header">The header text of the tab, which is displayed on the tab itself.</param>
public abstract partial class TabViewModelBase(TranslatableString header, Symbol? icon = null) : ObservableObject, ITab
{
    /// <summary>
    ///     Command that is executed when the user attempts to close the tab.
    /// </summary>
    public abstract IRelayCommand CloseCommand { get; }

    /// <summary>
    ///     The header text of the tab, which is displayed on the tab itself.
    /// </summary>
    [ObservableProperty]
    public partial TranslatableString Header { get; protected set; } = header;

    [ObservableProperty]
    public virtual partial string? ToolTip { get; set; }

    /// <summary>
    ///     The Symbol icon that is displayed on the tab.
    /// </summary>
    [ObservableProperty]
    public partial Symbol? Icon { get; protected set; } = icon;

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

    /// <summary>
    ///     Raises the <see cref="CloseRequested" /> event to signal that the tab should be closed.
    /// </summary>
    protected void RequestClose()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Event that is raised when the tab requests to be closed.
    /// </summary>
    public event EventHandler? CloseRequested;
}