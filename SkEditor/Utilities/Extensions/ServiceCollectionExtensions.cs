using Microsoft.Extensions.DependencyInjection;
using SkEditor.Features;

namespace SkEditor.Utilities.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddEditorFeatures(this IServiceCollection services)
    {
        services.AddTransient<IEditorFeature, SaveIndicatorFeature>();
        services.AddTransient<IEditorFeature, ZoomFeature>();
        services.AddTransient<IEditorFeature, EditorOptionsFeature>();
        services.AddTransient<IEditorFeature, EditorShortcutsFeature>();
        services.AddTransient<IEditorFeature, TextMateFeature>();
        services.AddTransient<IEditorFeature, HexColorFeature>();
    }
}