using System;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Analyzing;
using SkEditor.Analyzing.Aot;
using SkEditor.Analyzing.Bootstrapping;
using SkEditor.Analyzing.Processes;
using SkEditor.Analyzing.Protocol;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Contracts.Services.Security;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.DataTemplates;
using SkEditor.Features;
using SkEditor.Features.Editing;
using SkEditor.Services;
using SkEditor.Services.Files;
using SkEditor.Services.Files.Features;
using SkEditor.Services.Localization;
using SkEditor.Services.Security;
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
using SkEditor.Utilities.StatusBar;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Controls.FileTree;
using SkEditor.ViewModels.Controls.Sidebar.Panes;
using SkEditor.ViewModels.Controls.StatusBar.Panes;
using SkEditor.ViewModels.Controls.StatusBar.Panes.Terminal;
using SkEditor.ViewModels.Pages;
using SkEditor.ViewModels.Tabs;
using SkEditor.ViewModels.Windows;
using SkEditor.Views.Controls.Sidebar;
using SkEditor.Views.Controls.StatusBar.Panes;
using SkEditor.Views.Pages;
using SkEditor.Views.Windows;

namespace SkEditor.Utilities.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddCoreServices()
        {
            services.AddSingleton<IconProvider>();
            services.AddSingleton<NamedPipeServer>();
            services.AddSingleton<IStartupService, StartupService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<IWelcomeService, WelcomeService>();
            services.AddSingleton<ITranslationService, TranslationService>();
            services.AddSingleton<IDiscordIntegrationService, DiscordIntegrationService>();
            services.AddSingleton<IClipboardService, ClipboardService>();
            services.AddSingleton<ILinkService, LinkService>();

            services.AddSingleton<IRemoteConnectionService, RemoteConnectionService>();
            services.AddSingleton<ICredentialStoreFactory, CredentialStoreFactory>();
            services.AddHttpClient<IPublishService, PublishService>();

            services.AddSingleton<IMenuService, MenuService>();
            services.AddMainMenuProviders();

            services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
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
            services.AddSingleton<IWindowCloseService, WindowCloseService>();
            services.AddSingleton<ShortcutsService>();
            services.AddSingleton<SplashScreenService>();
        }

        public void AddFileServices()
        {
            services.AddSingleton<ITabService, TabService>();
            services.AddSingleton<ITabCloseService, TabCloseService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IFileTabFactory, FileTabFactory>();
            services.AddSingleton<IZoomService, ZoomService>();
            services.AddSingleton<IFilePickerService, FilePickerService>();
            services.AddSingleton<IRecentFileService, RecentFileService>();
            services.AddSingleton<DragDropService>();
        }

        public void AddViewModels()
        {
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<MainMenuViewModel>();
            services.AddTransient<RemoteConnectionViewModel>();
            services.AddTransient<FileTreeViewModel>();
            services.AddTransient<ThemePageViewModel>();
            services.AddTransient<WhatsNewPageViewModel>();
            services.AddTransient<WelcomePageViewModel>();
            services.AddTransient<PublishWindowViewModel>();
            services.AddTransient<AboutWindowViewModel>();
            services.AddTransient<GoToLineWindowViewModel>();
        }

        public void AddViews()
        {
            services.AddTransient<MainWindow>();
            services.AddTransient<SplashScreen>();
            services.AddTransient<SettingsTabView>();
            services.AddTransient<WhatsNewPage>();

            services.AddTransientWithFactory<PublishWindow>();
            services.AddTransientWithFactory<GoToLineWindow>();
            services.AddTransientWithFactory<AboutWindow>();
            services.AddTransientWithFactory<ManageRemoteConnectionsWindow>();
        }

        public void AddEditorFeatures()
        {
            services.AddSingleton<IFileFeatureService, FileFeatureService>();
            services.AddTransient<IEditorFeature, SaveIndicatorFeature>();
            services.AddTransient<IEditorFeature, AutoSaveFeature>();
            services.AddTransient<IEditorFeature, ZoomFeature>();
            services.AddTransient<IEditorFeature, EditorOptionsFeature>();
            services.AddTransient<IEditorFeature, EditorShortcutsFeature>();
            services.AddTransient<IEditorFeature, TextMateFeature>();
            services.AddTransient<IEditorFeature, ColorPreviewFeature>();
            services.AddTransient<IEditorFeature, EditorSmoothScrollFeature>();
            services.AddTransient<IEditorFeature, EditorCommandsFeature>();
            services.AddTransient<IEditorFeature, EditorRedrawFeature>();
            services.AddTransient<IEditorFeature, EditorFocusFeature>();
            services.AddTransient<IEditorFeature, EditorGoToLineFeature>();
            services.AddTransient<IEditorFeature, AutoIndentFeature>();
            services.AddTransient<IEditorFeature, SmartAutoPairingFeature>();
            services.AddTransient<IEditorFeature, SelectionOccurencesFeature>();
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
            services.AddSingleton<ISettingsSectionProvider, AnalyzerSectionProvider>();
        }

        private void AddMainMenuProviders()
        {
            services.AddSingleton<IMenuProvider, FileMenuProvider>();
            services.AddSingleton<IMenuProvider, EditMenuProvider>();
            services.AddSingleton<IMenuProvider, ToolsMenuProvider>();
            services.AddSingleton<IMenuProvider, HelpMenuProvider>();
            services.AddSingleton<IMenuProvider, DevMenuProvider>();
            services.AddSingleton<IMenuProvider, SettingsMenuProvider>();
        }

        public void AddTabs()
        {
            services.AddTransientWithFactory<SettingsTabViewModel>();
            services.AddTransientWithFactory<DocumentationTabViewModel>();

            services.AddSingleton<TabViewLocator>(_ =>
            {
                var locator = new TabViewLocator();

                locator.Register<SettingsTabViewModel, SettingsTabView>();
                locator.Register<DocumentationTabViewModel, DocumentationTabView>();
                locator.Register<FileTabViewModel, FileTabView>();

                return locator;
            });
        }

        public void AddTransientWithFactory<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            T>() where T : class
        {
            services.AddTransient<T>();
            services.AddSingleton<Func<T>>(sp => sp.GetRequiredService<T>);
        }

        public void AddSidebar()
        {
            services.AddSingleton<ISidebarPanelViewModel, ExplorerViewModel>();
            services.AddSingleton<ISidebarPanelViewModel, RemoteExplorerViewModel>();
            // services.AddSingleton<ISidebarPanelViewModel, SearchPaneViewModel>();

            services.AddSingleton<SidebarViewModel>();

            SidebarPaneViewLocator viewLocator = new();
            viewLocator.Register<ExplorerViewModel, ExplorerView>();
            viewLocator.Register<RemoteExplorerViewModel, RemoteExplorerView>();
            // viewLocator.Register<SearchPaneViewModel, SearchPaneView>();

            services.AddSingleton(viewLocator);
        }

        public void AddStatusBar()
        {
            services.AddSingleton<StatusBarViewModel>();
            services.AddDrawer();

            services.AddSingleton<IStatusBarProvider, EditorInfoStatusBarProvider>();

            services.AddSingleton<EditorErrorStatusBarProvider>();
            services.AddSingleton<IStatusBarProvider>(sp => sp.GetRequiredService<EditorErrorStatusBarProvider>());
        }

        private void AddDrawer()
        {
            services.AddSingleton<BottomDrawerViewModel>();

            services.AddSingleton<IDrawerPaneViewModel, ErrorListPaneViewModel>();
            services.AddSingleton<IDrawerPaneViewModel, TerminalPaneViewModel>();

            DrawerPaneViewLocator viewLocator = new();
            viewLocator.Register<ErrorListPaneViewModel, ErrorListPaneView>();
            viewLocator.Register<TerminalPaneViewModel, TerminalPaneView>();
            viewLocator.Register<TerminalToolbarViewModel, TerminalToolbarView>();

            services.AddSingleton(viewLocator);
        }

        public void AddAnalyzer()
        {
            services.AddSingleton<IAnalyzerBootstrapper, AnalyzerBootstrapper>();
            services.AddSingleton<IAnalyzerAotGenerator, AnalyzerAotGenerator>();
            services.AddSingleton<IAnalyzerProcessBuilder, AnalyzerProcessBuilder>();
            services.AddSingleton<IAnalyzerProcessRunner, AnalyzerProcessRunner>();
            services.AddSingleton<IAnalyzerProtocolHandler, AnalyzerProtocolHandler>();

            services.AddSingleton<IAnalyzer, Analyzer>();
            services.AddTransient<IEditorFeature, AnalyzerFeature>();
        }
    }
}