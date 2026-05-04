using System;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.DataTemplates;
using SkEditor.Features;
using SkEditor.Models.Tabs;
using SkEditor.Services;
using SkEditor.Services.Files;
using SkEditor.Services.Files.Features;
using SkEditor.Services.Localization;
using SkEditor.Services.Settings;
using SkEditor.Services.Windowing;
using SkEditor.Utilities.Editor.Commands;
using SkEditor.Utilities.Menu;
using SkEditor.Utilities.Menu.Providers;
using SkEditor.Utilities.Publishing;
using SkEditor.Utilities.Settings;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Settings.Providers.Choice;
using SkEditor.Utilities.Settings.Providers.Sections;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Controls.Sidebar.Panes;
using SkEditor.ViewModels.Pages;
using SkEditor.ViewModels.Windows;
using SkEditor.Views.Controls.Sidebar;
using SkEditor.Views.Pages;
using SkEditor.Views.Windows;
using RemoteSidebarViewModel = SkEditor.ViewModels.Controls.Sidebar.Panes.RemoteSidebarViewModel;

namespace SkEditor.Utilities.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddCoreServices()
        {
            services.AddSingleton<IStartupService, StartupService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<IconProvider>();
            services.AddSingleton<CrashChecker>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<IWelcomeService, WelcomeService>();
            services.AddSingleton<ITranslationService, TranslationService>();
            services.AddSingleton<IDiscordIntegrationService, DiscordIntegrationService>();
            services.AddSingleton<IClipboardService, ClipboardService>();
            services.AddSingleton<NamedPipeServer>();

            services.AddSingleton<IRemoteConnectionService, RemoteConnectionService>();
            services.AddHttpClient<IPublishService, PublishService>();

            services.AddSingleton<IMenuService, MenuService>();
            services.AddMainMenuProviders();
        }

        public void AddWindowing()
        {
            services.AddSingleton<MainWindowFactory>();
            services.AddSingleton<WindowRegistry>();
            services.AddSingleton<WindowInstanceService>();
            services.AddSingleton<WindowStateService>();
            services.AddSingleton<WindowErrorService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IWindowDialogService, WindowDialogService>();
            services.AddSingleton<ShortcutsService>();
        }

        public void AddFileServices()
        {
            services.AddSingleton<ITabService, TabService>();
            services.AddSingleton<ITabCloseService, TabCloseService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IFileTabFactory, FileTabFactory>();
            services.AddSingleton<IZoomService, ZoomService>();
            services.AddSingleton<IFilePickerService, FilePickerService>();
            services.AddSingleton<DragDropService>();
        }

        public void AddViewModels()
        {
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<MainMenuViewModel>();
            services.AddTransient<SidebarViewModel>();
            services.AddTransient<RemoteConnectionViewModel>();
            services.AddTransient<RemoteSidebarViewModel>();
            services.AddTransient<FileTreeViewModel>();
            services.AddTransient<ThemePageViewModel>();
            services.AddTransient<WhatsNewPageViewModel>();
            services.AddTransient<WelcomePageViewModel>();
            services.AddTransient<SettingsPageViewModel>();
            services.AddTransient<PublishWindowViewModel>();
        }

        public void AddViews()
        {
            services.AddTransient<MainWindow>();
            services.AddTransient<SplashScreen>();
            services.AddTransient<RemoteConnectionWindow>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<WhatsNewPage>();

            services.AddTransient<PublishWindow>();
            services.AddSingleton<Func<PublishWindow>>(sp => sp.GetRequiredService<PublishWindow>);

            services.AddTransient<GoToLineWindow>();
            services.AddSingleton<Func<GoToLineWindow>>(sp => sp.GetRequiredService<GoToLineWindow>);

            services.AddTransient<SettingsTab>();
            services.AddSingleton<Func<SettingsTab>>(sp => sp.GetRequiredService<SettingsTab>);
        }

        public void AddEditorFeatures()
        {
            services.AddSingleton<IFileFeatureService, FileFeatureService>();
            services.AddTransient<IEditorFeature, SaveIndicatorFeature>();
            services.AddTransient<IEditorFeature, ZoomFeature>();
            services.AddTransient<IEditorFeature, EditorOptionsFeature>();
            services.AddTransient<IEditorFeature, EditorShortcutsFeature>();
            services.AddTransient<IEditorFeature, TextMateFeature>();
            services.AddTransient<IEditorFeature, HexColorFeature>();
            services.AddTransient<IEditorFeature, EditorSmoothScrollFeature>();
            services.AddTransient<IEditorFeature, EditorCommandsFeature>();
        }

        public void AddEditorCommands()
        {
            services.AddSingleton<IEditorCommand, CommentCommand>();
            services.AddSingleton<IEditorCommand, DuplicateCommand>();
            services.AddSingleton<IEditorCommand, TrimWhitespaceCommand>();
        }

        public void AddConfigs(BinarySettings binarySettings)
        {
            services.AddSingleton<IConfigService, ConfigService>();
            services.AddSingleton<LegacyConfigMigrator>();

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
            services.AddSingleton<ISettingsSectionProvider, ExperimentsSectionProvider>();
            services.AddSingleton<ISettingsSectionProvider, AdvancedSectionProvider>();
        }

        private void AddMainMenuProviders()
        {
            services.AddSingleton<IMenuProvider, FileMenuProvider>();
            services.AddSingleton<IMenuProvider, EditMenuProvider>();
            services.AddSingleton<IMenuProvider, SettingsMenuProvider>();
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

        public void AddSidebar()
        {
            services.AddTransient<ISidebarPanelViewModel, ExplorerViewModel>();

            services.AddSingleton<SidebarViewModel>();

            SidebarPaneViewLocator viewLocator = new();
            viewLocator.Register<ExplorerViewModel, ExplorerView>();
            services.AddSingleton(viewLocator);
        }
    }
}