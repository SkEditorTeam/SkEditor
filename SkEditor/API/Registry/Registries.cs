using System.Collections.Generic;
using SkEditor.API.Model;

namespace SkEditor.API.Registry;

/// <summary>
/// Holds every registry used by the application.
/// </summary>
public static class Registries
{
    
    public static readonly Registry<ConnectionData> Connections = new();
    
    public static void Unload(IAddon addon)
    {
        var fields = typeof(Registries).GetFields();
        foreach (var field in fields)
        {
            if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Registry<>))
            {
                var registry = field.GetValue(null);
                var method = registry.GetType().GetMethod("Unload");
                method.Invoke(registry, [addon]);
            }
        }
    }
}