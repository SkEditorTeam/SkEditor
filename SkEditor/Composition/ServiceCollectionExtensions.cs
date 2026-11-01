using System;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Analyzing;
using SkEditor.Analyzing.Aot;
using SkEditor.Analyzing.Bootstrapping;
using SkEditor.Analyzing.Downloading;
using SkEditor.Analyzing.Processes;
using SkEditor.Analyzing.Protocol;
using SkEditor.Appearance.Icons;
using SkEditor.Appearance.Themes;
using SkEditor.Configuration;
using SkEditor.Configuration.Abstractions;
using SkEditor.Configuration.Configs;
using SkEditor.Configuration.Providers.Choice;
using SkEditor.Configuration.Providers.Sections;
using SkEditor.Configuration.UI;
using SkEditor.Core.Common;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Localization;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Core.Security;
using SkEditor.Core.System;
using SkEditor.Core.Updates;
using SkEditor.Documentation;
using SkEditor.Documentation.Providers;
using SkEditor.Documentation.UI;
using SkEditor.Editor.Commands;
using SkEditor.Editor.Completions;
using SkEditor.Editor.Completions.Providers;
using SkEditor.Editor.Features;
using SkEditor.Editor.Features.Decorations;
using SkEditor.Editor.Features.Diagnostics;
using SkEditor.Editor.Features.Editing;
using SkEditor.Editor.Features.Saving;
using SkEditor.Editor.Features.View;
using SkEditor.Editor.Features.View.Zoom;
using SkEditor.Editor.Syntax;
using SkEditor.Editor.UI.Tabs;
using SkEditor.Editor.UI.Windows;
using SkEditor.Integrations.Discord;
using SkEditor.Integrations.Publishing;
using SkEditor.Integrations.Publishing.UI;
using SkEditor.Layout.Menu;
using SkEditor.Layout.Menu.Providers;
using SkEditor.Layout.Menu.UI;
using SkEditor.Layout.Sidebar;
using SkEditor.Layout.Sidebar.UI;
using SkEditor.Layout.Sidebar.UI.Panes;
using SkEditor.Layout.StatusBar.Providers;
using SkEditor.Layout.StatusBar.UI;
using SkEditor.Layout.StatusBar.UI.Abstractions;
using SkEditor.Layout.StatusBar.UI.Panes.Errors;
using SkEditor.Layout.StatusBar.UI.Panes.Logs;
using SkEditor.Layout.StatusBar.UI.Panes.Terminal;
using SkEditor.Matcher;
using SkEditor.Welcome;
using SkEditor.Welcome.UI;
using SkEditor.Windowing;
using SkEditor.Windowing.Errors;
using SkEditor.Windowing.UI;
using SkEditor.Windowing.UI.About;
using SkEditor.Workspace.Files;
using SkEditor.Workspace.Projects;
using SkEditor.Workspace.Remote;
using SkEditor.Workspace.Remote.UI;
using SkEditor.Workspace.Session;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Templates;
using SkEditor.Workspace.UI.Tree;

namespace SkEditor.Composition;

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
            services.AddSingleton<ISyntaxService, SyntaxService>();
            services.AddSingleton<IWelcomeService, WelcomeService>();
            services.AddSingleton<ITranslationService, TranslationService>();
            services.AddSingleton<IDiscordIntegrationService, DiscordIntegrationService>();
            services.AddSingleton<IClipboardService, ClipboardService>();
            services.AddSingleton<ILinkService, LinkService>();
            services.AddSingleton<IProjectService, ProjectService>();
            services.AddSingleton<ISidebarService, SidebarService>();
            services.AddSingleton<IUpdateService, UpdateService>();

            services.AddSingleton<IRemoteConnectionService, RemoteConnectionService>();
            services.AddSingleton<ICredentialStoreFactory, CredentialStoreFactory>();
            services.AddHttpClient<IPublishService, PublishService>();
            services.AddHttpClient<IDocumentationService, DocumentationService>();

            services.AddSingleton<IMenuService, MenuService>();
            services.AddMainMenuProviders();

            services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

            services.AddDocumentationProviders();
            
            services.AddHttpClient(HttpClients.SkEditor, client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "SkEditor");
            });
        }

        public void AddWindowing()
        {
            services.AddSingleton<MainWindowFactory>();
            services.AddSingleton<IWindowRegistry, WindowRegistry>();
            services.AddSingleton<WindowInstanceService>();
            services.AddSingleton<WindowStateService>();
            services.AddSingleton<WindowErrorService>();
            services.AddSingleton<ICrashService, CrashService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IWindowDialogService, WindowDialogService>();
            services.AddSingleton<IWindowCloseService, WindowCloseService>();
            services.AddSingleton<ISessionRestoreService, SessionRestoreService>();
            services.AddSingleton<ShortcutsService>();
            services.AddSingleton<SplashScreenService>();
        }

        public void AddFileServices()
        {
            services.AddSingleton<ITabService, TabService>();
            services.AddSingleton<IFileTabService, FileTabService>();
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
            services.AddTransient<IEditorFeature, AltKeyHandlingFeature>();
            services.AddTransient<IEditorFeature, CompletionFeature>();
            services.AddTransient<IEditorFeature, ChangeDetectFeature>();
        }

        public void AddEditorCommands()
        {
            services.AddSingleton<IEditorCommandHandler, CommentCommand>();
            services.AddSingleton<IEditorCommandHandler, DuplicateCommand>();
            services.AddSingleton<IEditorCommandHandler, TrimWhitespaceCommand>();
            services.AddSingleton<IEditorCommandHandler, TransformCommand>();
            services.AddSingleton<IEditorCommandHandler, ConvertIndentationCommand>();
        }

        public void AddEditorCompletions()
        {
            services.AddSingleton<ISkriptSyntaxService, SkriptSyntaxService>();
            
            services.AddSingleton<ICompletionProvider, SectionCompletionProvider>();
            services.AddSingleton<ICompletionProvider, EventCompletionProvider>();
            services.AddSingleton<ICompletionProvider, VariableCompletionProvider>();
            services.AddSingleton<ICompletionProvider, EffectCompletionProvider>();
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

            services.AddChoiceProviders();
            services.AddSectionProviders();

            return;

            void AddConfig<TConfig>(TConfig config) where TConfig : class, IConfig
            {
                services.AddSingleton(config);
                services.AddSingleton<IConfig>(sp => sp.GetRequiredService<TConfig>());
            }
        }

        public void AddTabs()
        {
            services.AddTransientWithFactory<SettingsTabViewModel>();
            services.AddTransientWithFactory<DocumentationTabViewModel>();

            services.AddSingleton<TabViewLocator>(_ =>
            {
                TabViewLocator locator = new();

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

        public void AddAnalyzer()
        {
            services.AddSingleton<IAnalyzerBootstrapper, AnalyzerBootstrapper>();
            services.AddSingleton<IAnalyzerAotGenerator, AnalyzerAotGenerator>();
            services.AddSingleton<IAnalyzerProcessBuilder, AnalyzerProcessBuilder>();
            services.AddSingleton<IAnalyzerProcessRunner, AnalyzerProcessRunner>();
            services.AddSingleton<IAnalyzerProtocolHandler, AnalyzerProtocolHandler>();
            services.AddSingleton<IAnalyzerInitializer, AnalyzerInitializer>();
            services.AddSingleton<IAnalyzerDownloadHelper, AnalyzerDownloadHelper>();

            services.AddSingleton<IAnalyzer, Analyzer>();
            services.AddTransient<IEditorFeature, AnalyzerFeature>();
            services.AddTransient<IEditorFeature, AnalyzerTooltipFeature>();
        }

        private void AddChoiceProviders()
        {
            services.AddSingleton<LanguageChoiceProvider>();
            services.AddSingleton<ThemeChoiceProvider>();
            services.AddSingleton<StartupModeChoiceProvider>();
            services.AddSingleton<IndentAmountChoiceProvider>();
            services.AddSingleton<IndentTypeChoiceProvider>();
            services.AddSingleton<SyntaxThemeChoiceProvider>();
            services.AddSingleton<SyntaxThemeSkriptChoiceProvider>();
            services.AddSingleton<SyntaxGrammarChoiceProvider>();
            services.AddSingleton<SyntaxGrammarSkriptChoiceProvider>();
            services.AddSingleton<FontFamilyChoiceProvider>();
        }

        private void AddSectionProviders()
        {
            services.AddSingleton<ISettingsSectionProvider, GeneralSectionProvider>();
            services.AddSingleton<ISettingsSectionProvider, EditorSectionProvider>();
            services.AddSingleton<ISettingsSectionProvider, AppearanceSectionProvider>();
            services.AddSingleton<ISettingsSectionProvider, SyntaxSectionProvider>();
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

        private void AddDrawer()
        {
            services.AddSingleton<BottomDrawerViewModel>();

            services.AddSingleton<IDrawerPaneViewModel, ErrorListPaneViewModel>();
            services.AddSingleton<IDrawerPaneViewModel, TerminalPaneViewModel>();
            services.AddSingleton<IDrawerPaneViewModel, LogsPaneViewModel>();

            DrawerPaneViewLocator viewLocator = new();
            viewLocator.Register<ErrorListPaneViewModel, ErrorListPaneView>();
            viewLocator.Register<TerminalPaneViewModel, TerminalPaneView>();
            viewLocator.Register<TerminalToolbarViewModel, TerminalToolbarView>();
            viewLocator.Register<LogsPaneViewModel, LogsPaneView>();
            viewLocator.Register<LogsToolbarViewModel, LogsToolbarView>();

            services.AddSingleton(viewLocator);
        }

        private void AddDocumentationProviders()
        {
            services.AddSingleton<IDocumentationProvider, SkDocsProvider>();
            services.AddSingleton<IDocumentationProvider, SkUnityProvider>();
        }
    }
}