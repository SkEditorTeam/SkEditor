using Microsoft.Extensions.DependencyInjection;
using SkEditor.Features;

namespace SkEditor.Utilities.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddEditorFeatures(this IServiceCollection services)
    {
        services.AddSingleton<IEditorFeature, SaveIndicatorFeature>();
        services.AddSingleton<IEditorFeature, ZoomFeature>();
    }
}