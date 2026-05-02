using Microsoft.Extensions.DependencyInjection;
using SkEditor.DataTemplates;
using SkEditor.Features;
using SkEditor.Models.Tabs;
using SkEditor.Services.Settings;
using SkEditor.Utilities.Settings;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Settings.Providers.Choice;
using SkEditor.Utilities.Settings.Providers.Sections;
using SkEditor.Views.Pages;

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

            services.AddChoiceProviders();
            services.AddSectionProviders();

            return;

            void AddConfig<TConfig>(TConfig config) where TConfig : class, IConfig
            {
                services.AddSingleton(config);
                services.AddSingleton<IConfig>(sp => sp.GetRequiredService<TConfig>());
            }
        }

        private void AddChoiceProviders()
        {
            services.AddSingleton<LanguageChoiceProvider>();
            services.AddSingleton<ThemeChoiceProvider>();
            services.AddSingleton<StartupModeChoiceProvider>();
            services.AddSingleton<IndentAmountChoiceProvider>();
            services.AddSingleton<IndentTypeChoiceProvider>();
        }

        private void AddSectionProviders()
        {
            services.AddSingleton<ISettingsSectionProvider, GeneralSectionProvider>();
            services.AddSingleton<ISettingsSectionProvider, EditorSectionProvider>();
            services.AddSingleton<ISettingsSectionProvider, AppearanceSectionProvider>();
            services.AddSingleton<ISettingsSectionProvider, IntegrationsSectionProvider>();
        }

        public void AddTabViewLocator()
        {
            services.AddSingleton<TabViewLocator>(sp =>
            {
                TabViewLocator locator = new();

                locator.Register<SettingsTab>(tab =>
                {
                    SettingsPage page = sp.GetRequiredService<SettingsPage>();
                    page.DataContext = ((SettingsTab)tab).PageViewModel;
                    return page;
                });

                locator.Register<FileTab>(tab => new FileTabView { DataContext = tab });

                return locator;
            });
        }
    }
}