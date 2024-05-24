using SkEditor.API.Model;

namespace SkEditor.API.Registry;

/// <summary>
/// Holds every registry used by the application.
/// </summary>
public static class Registries
{
    
    public static readonly Registry<ConnectionData> Connections = new();
    
}