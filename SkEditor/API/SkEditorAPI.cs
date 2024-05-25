namespace SkEditor.API;

/// <summary>
/// The main API class for SkEditor, that regroups all the API classes.
/// </summary>
public static class SkEditorAPI
{
    
    /// <summary>
    /// Get the Logs API.
    /// </summary>
    public static ILogs Logs => new Logs();
    
    /// <summary>
    /// Get the Windows API.
    /// </summary>
    public static IWindows Windows => new Windows();
    
    /// <summary>
    /// Get the Core API.
    /// </summary>
    public static ICore Core => new Core();
    
    /// <summary>
    /// Get the Events API.
    /// </summary>
    public static IEvents Events => new Events();
    
}