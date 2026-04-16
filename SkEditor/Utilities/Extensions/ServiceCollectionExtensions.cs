using Microsoft.Extensions.DependencyInjection;
using SkEditor.Features;

namespace SkEditor.Utilities.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddEditorFeatures(this IServiceCollection services)
    {
        services.AddSingleton<IEditorFeature, SaveIndicatorFeature>();
        services.AddSingleton<IEditorFeature, ZoomFeature>();
        services.AddSingleton<IEditorFeature, EditorOptionsFeature>();
        services.AddSingleton<IEditorFeature, EditorShortcutsFeature>();
        services.AddSingleton<IEditorFeature, TextMateFeature>();
        services.AddSingleton<IEditorFeature, HexColorFeature>();
    }
}