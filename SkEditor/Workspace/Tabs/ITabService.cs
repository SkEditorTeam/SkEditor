using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Workspace.Tabs;

/// <summary>
///     Used to manage all the opened tabs.
/// </summary>
public interface ITabService
{
    /// <summary>
    ///     All the opened tabs, including both main and split tabs.
    /// </summary>
    IEnumerable<ITab> AllTabs { get; }

    /// <summary>
    ///     The collection of main tabs, which are opened in the main view.
    /// </summary>
    ObservableCollection<ITab> MainTabs { get; }

    ITab? SelectedMainTab { get; set; }
    ITab? SelectedSplitTab { get; set; }

    /// <summary>
    ///     The currently selected tab. Can be null if no tabs are opened.
    /// </summary>
    ITab? SelectedTab { get; set; }

    /// <summary>
    ///     The collection of split tabs, which are opened in the split view.
    /// </summary>
    ObservableCollection<ITab> SplitTabs { get; }

    /// <summary>
    ///     Switches to the tab at the given index. Does nothing if the index is out of range.
    /// </summary>
    /// <param name="index">The index of the tab to switch to.</param>
    void SwitchTab(int index);

    /// <summary>
    ///     Opens the given tab.
    /// </summary>
    /// <param name="tab">The tab to open.</param>
    /// <param name="select">Whether to select the tab after opening.</param>
    void OpenTab(ITab tab, bool select = true);

    /// <summary>
    ///     Opens a new tab created by the given factory.
    /// </summary>
    /// <param name="factory">The factory to create the tab.</param>
    /// <param name="allowMultiple">
    ///     Whether to allow multiple tabs of the same type. If false, it will try to find an existing
    ///     tab of the same type and select it instead of opening a new one.
    /// </param>
    /// <typeparam name="T">The type of the tab to open.</typeparam>
    void OpenUniqueTab<T>(Func<T> factory, bool allowMultiple = false) where T : ITab;

    /// <summary>
    ///     Closes the given tab.
    /// </summary>
    /// <param name="tab">The tab to close.</param>
    /// <remarks>This force-closes the tab and normally should only be used after the closing is handled by the tab itself.</remarks>
    void CloseTab(ITab tab);

    /// <summary>
    ///     Splits the given tab into the split view. Nothing happens if the tab is already in the split view or is the only
    ///     tab.
    /// </summary>
    void SplitTab(ITab tab);

    /// <summary>
    ///     Merges the given tab back into the main view.
    /// </summary>
    void MergeTab(ITab tab);

    /// <summary>
    ///     Raised when the selected tab changes.
    /// </summary>
    event Action<ITab?>? SelectedTabChanged;

    /// <summary>
    ///     Raised when a tab is closed.
    /// </summary>
    event Action<ITab>? TabClosed;
}