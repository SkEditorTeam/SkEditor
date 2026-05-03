namespace SkEditor.Models.Tabs;

/// <summary>
///     The type of closing to perform when closing multiple tabs.
/// </summary>
public enum CloseType
{
    /// <summary>
    ///     Closes all tabs.
    /// </summary>
    All,

    /// <summary>
    ///     Closes all tabs except the currently selected one.
    /// </summary>
    AllButCurrent,

    /// <summary>
    ///     Closes all unsaved tabs.
    /// </summary>
    Unsaved,

    /// <summary>
    ///     Closes all tabs to the left of the currently selected one.
    /// </summary>
    Left,

    /// <summary>
    ///     Closes all tabs to the right of the currently selected one.
    /// </summary>
    Right
}