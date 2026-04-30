using Microsoft.Extensions.DependencyInjection;
using SkEditor.Features;
using SkEditor.Services.Settings;
using SkEditor.Utilities.Settings;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Settings.Providers;

namespace SkEditor.Utilities.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddEditorFeatures()
        {
            services.AddTransient<IEditorFeature, SaveIndicatorFeature>();
            services.AddTransient<IEditorFeature, ZoomFeature>();
            services.AddTransient<IEditorFeature, EditorOptionsFeature>();
            services.AddTransient<IEditorFeature, EditorShortcutsFeature>();
            services.AddTransient<IEditorFeature, TextMateFeature>();
            services.AddTransient<IEditorFeature, HexColorFeature>();
            services.AddTransient<IEditorFeature, EditorSmoothScrollFeature>();
        }

        public void AddConfigs(BinarySettings binarySettings)
        {
            AddConfig(new AppSettings());
            AddConfig(new ConnectionsConfig());
            AddConfig(new ExperimentsConfig());
            AddConfig(new SecretsConfig());
            AddConfig(new StateConfig());
            AddConfig(binarySettings);

            services.AddSingleton<SettingsObserverService>();

            services.AddSingleton<LanguageSettingsProvider>();
            services.AddSingleton<ThemeSettingsProvider>();
            services.AddSingleton<StartupModeSettingsProvider>();
            services.AddSingleton<IndentAmountProvider>();
            services.AddSingleton<IndentTypeProvider>();

            return;

            void AddConfig<TConfig>(TConfig config) where TConfig : class, IConfig
            {
                services.AddSingleton(config);
                services.AddSingleton<IConfig>(sp => sp.GetRequiredService<TConfig>());
            }
        }
    }
}