using System;
using System.Collections.ObjectModel;
using SkEditor.Models.Tabs;

namespace SkEditor.Contracts.Services.Files;

/// <summary>
///     Used to manage all the opened tabs.
/// </summary>
public interface ITabService
{
    /// <summary>
    ///     The collection of all the opened tabs.
    /// </summary>
    ObservableCollection<Tab> Tabs { get; }

    /// <summary>
    ///     The currently selected tab. Can be null if no tabs are opened.
    /// </summary>
    Tab? SelectedTab { get; set; }

    /// <summary>
    ///     Raised when the selected tab changes.
    /// </summary>
    event Action<Tab?>? SelectedTabChanged;

    /// <summary>
    ///     Opens the given tab.
    /// </summary>
    /// <param name="tab">The tab to open.</param>
    void OpenTab(Tab tab);

    /// <summary>
    ///     Opens a new tab created by the given factory.
    /// </summary>
    /// <param name="factory">The factory to create the tab.</param>
    /// <param name="allowMultiple">
    ///     Whether to allow multiple tabs of the same type. If false, it will try to find an existing
    ///     tab of the same type and select it instead of opening a new one.
    /// </param>
    /// <typeparam name="T">The type of the tab to open.</typeparam>
    void OpenUniqueTab<T>(Func<T> factory, bool allowMultiple = false) where T : Tab;

    /// <summary>
    ///     Closes the given tab.
    /// </summary>
    /// <param name="tab">The tab to close.</param>
    /// <remarks>This force-closes the tab and normally should only be used after the closing is handled by the tab itself.</remarks>
    void CloseTab(Tab tab);
}