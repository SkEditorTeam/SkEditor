namespace SkEditor.API;

/// <summary>
/// The main API class for SkEditor, that regroups all the API classes.
/// </summary>
public static class SkEditorAPI
{
    
    /// <summary>
    /// Get the Logs API.
    /// </summary>
    public static readonly ILogs Logs = new Logs();
    
    /// <summary>
    /// Get the Windows API.
    /// </summary>
    public static readonly IWindows Windows = new Windows();
    
    /// <summary>
    /// Get the Core API.
    /// </summary>
    public static readonly ICore Core = new Core();
    
    /// <summary>
    /// Get the Events API.
    /// </summary>
    public static readonly IEvents Events = new Events();
    
    /// <summary>
    /// Get the Addons API.
    /// </summary>
    public static readonly IAddons Addons = new Addons();

    /// <summary>
    /// Get the Files API.
    /// </summary>
    public static readonly IFiles Files = new Files();

}