using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Controls.Docs;
using SkEditor.Utilities;
using SkEditor.Utilities.Editor;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Projects;
using SkEditor.Utilities.Styling;
using SkEditor.Utilities.Syntax;
using SkEditor.Views;
using SkEditor.Views.Generators;
using SkEditor.Views.Generators.Gui;
using SkEditor.Views.Settings;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Controls;

public partial class MainMenuControl : UserControl
{
    public MainMenuControl()
    {
        InitializeComponent();

        ConfigureMenu();
        AssignCommands();
        AddMissingHotkeys();
    }

    private void ConfigureMenu()
    {
        SetVisibility();
        SkEditorAPI.Core.GetAppConfig().PropertyChanged += (_, _) => SetVisibility();
        return;

        void SetVisibility()
        {
            MenuItemDevTools.IsVisible = SkEditorAPI.Core.IsDeveloperMode();
        }
    }

    private void AssignCommands()
    {
        MenuItemNew.Command = new RelayCommand(FileHandler.NewFile);
        MenuItemOpen.Command = new AsyncRelayCommand(FileHandler.OpenFile);
        MenuItemOpenFolder.Command = new AsyncRelayCommand(async () => await ProjectOpener.OpenProject());
        MenuItemSave.Command = new RelayCommand(FileHandler.SaveFile);
        MenuItemSaveAs.Command = new RelayCommand(FileHandler.SaveAsFile);
        MenuItemSaveAll.Command = new RelayCommand(FileHandler.SaveAllFiles);
        MenuItemPublish.Command =
            new RelayCommand(() => new PublishWindow().ShowDialog(SkEditorAPI.Windows.GetMainWindow()));

        MenuItemClose.Command = new AsyncRelayCommand(FileCloser.CloseCurrentFile);
        MenuItemCloseAll.Command = new AsyncRelayCommand(FileCloser.CloseAllFiles);
        MenuItemCloseAllExceptCurrent.Command = new AsyncRelayCommand(FileCloser.CloseAllExceptCurrent);
        MenuItemCloseAllUnsaved.Command = new AsyncRelayCommand(FileCloser.CloseUnsaved);
        MenuItemCloseAllLeft.Command = new AsyncRelayCommand(FileCloser.CloseAllToTheLeft);
        MenuItemCloseAllRight.Command = new AsyncRelayCommand(FileCloser.CloseAllToTheRight);

        MenuItemCopy.Command = new RelayCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile().Editor?.Copy());
        MenuItemPaste.Command = new RelayCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile().Editor?.Paste());
        MenuItemCut.Command = new RelayCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile().Editor?.Cut());
        MenuItemUndo.Command = new RelayCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile().Editor?.Undo());
        MenuItemRedo.Command = new RelayCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile().Editor?.Redo());
        MenuItemDelete.Command = new RelayCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile().Editor?.Delete());
        MenuItemGoToLine.Command = new RelayCommand(() => ShowDialogIfEditorIsOpen(new GoToLineWindow()));
        MenuItemTrimWhitespaces.Command = new RelayCommand(() =>
            CustomCommandsHandler.OnTrimWhitespacesCommandExecuted(SkEditorAPI.Files.GetCurrentOpenedFile().Editor
                ?.TextArea));

        MenuItemDuplicate.Command = new RelayCommand(() =>
            CustomCommandsHandler.OnDuplicateCommandExecuted(SkEditorAPI.Files.GetCurrentOpenedFile().Editor
                ?.TextArea));
        MenuItemComment.Command = new RelayCommand(() =>
            CustomCommandsHandler.OnCommentCommandExecuted(SkEditorAPI.Files.GetCurrentOpenedFile().Editor?.TextArea));

        MenuItemRefreshSyntax.Command = new AsyncRelayCommand(async () => await SyntaxLoader.RefreshSyntaxAsync());
        MenuItemRefreshTheme.Command = new AsyncRelayCommand(async () => await ThemeEditor.ReloadCurrentTheme());

        MenuItemDocs.Command = new RelayCommand(AddDocsTab);
        MenuItemGenerateGui.Command = new RelayCommand(() => ShowDialogIfEditorIsOpen(new GuiGenerator(), false));
        MenuItemGenerateCommand.Command =
            new RelayCommand(() => ShowDialogIfEditorIsOpen(new CommandGenerator(), false));
        MenuItemRefactor.Command = new RelayCommand(() => ShowDialogIfEditorIsOpen(new RefactorWindow(), false));
        MenuItemColorSelector.Command =
            new RelayCommand(() => new ColorSelectionWindow().ShowDialog(SkEditorAPI.Windows.GetMainWindow()));

        MenuItemMarketplace.Command =
            new RelayCommand(() => new MarketplaceWindow().ShowDialog(SkEditorAPI.Windows.GetMainWindow()));

        MenuItemSettings.Command =
            new RelayCommand(() => new SettingsWindow().ShowDialog(SkEditorAPI.Windows.GetMainWindow()));
    }

    private static void ShowDialogIfEditorIsOpen(AppWindow window, bool openAsDialog = true)
    {
        if (!SkEditorAPI.Files.GetCurrentOpenedFile().IsEditor)
        {
            return;
        }

        if (openAsDialog)
        {
            window.ShowDialog(SkEditorAPI.Windows.GetMainWindow());
        }
        else
        {
            window.Show(SkEditorAPI.Windows.GetMainWindow());
        }
    }

    private void AddMissingHotkeys()
    {
        Loaded += (_, _) =>
        {
            SkEditorAPI.Windows.GetMainWindow().KeyDown += (_, e) =>
            {
                if (e is not { PhysicalKey: PhysicalKey.S, KeyModifiers: (KeyModifiers.Control | KeyModifiers.Alt) }
                    || !string.IsNullOrEmpty(e.KeySymbol))
                {
                    return;
                }

                MenuItemSaveAll.Command.Execute(null);
                e.Handled = true;
            };
        };
    }

    public static void AddDocsTab()
    {
        SymbolIconSource icon = new()
        {
            Symbol = Symbol.Book
        };
        SkEditorAPI.Files.AddCustomTab("Documentation", new DocumentationControl(), icon: icon);
    }

    public void ReloadAddonsMenus()
    {
        bool hasAnyMenu = false;
        AddonsMenuItem.Items.Clear();
        foreach (IAddon addon in SkEditorAPI.Addons.GetAddons(IAddons.AddonState.Enabled))
        {
            List<MenuItem> items = addon.GetMenuItems();
            if (items.Count <= 0)
            {
                continue;
            }

            hasAnyMenu = true;
            MenuItem menuItem = new()
            {
                Header = addon.Name,
                Icon = new IconSourceElement
                {
                    IconSource = addon.GetAddonIcon(),
                    Width = 20,
                    Height = 20
                }
            };

            if (addon.GetSettings().Count > 0)
            {
                menuItem.Items.Add(new MenuItem
                {
                    Header = Translation.Get("WindowTitleSettings"),
                    Command = new RelayCommand(() =>
                    {
                        new SettingsWindow().ShowDialog(MainWindow.Instance);
                        SettingsWindow.NavigateToPage(typeof(CustomAddonSettingsPage));
                        CustomAddonSettingsPage.Load(addon);
                    }),
                    Icon = new IconSourceElement
                    {
                        IconSource = new FluentAvalonia.UI.Controls.SymbolIconSource
                            { Symbol = FluentAvalonia.UI.Controls.Symbol.Setting, FontSize = 20 },
                        Width = 20,
                        Height = 20
                    }
                });
                menuItem.Items.Add(new Separator());
            }

            foreach (MenuItem sub in items)
            {
                menuItem.Items.Add(sub);
            }

            AddonsMenuItem.Items.Add(menuItem);
        }

        AddonsMenuItem.Items.Add(new Separator());
        AddonsMenuItem.Items.Add(new MenuItem
        {
            Header = Translation.Get("MenuHeaderManageAddons"),
            Command = new RelayCommand(() =>
            {
                new SettingsWindow().ShowDialog(MainWindow.Instance);
                SettingsWindow.NavigateToPage(typeof(AddonsPage));
            }),
            Icon = new IconSourceElement
            {
                IconSource = new FluentAvalonia.UI.Controls.SymbolIconSource
                {
                    Symbol = FluentAvalonia.UI.Controls.Symbol.Manage,
                    FontSize = 20
                },
                Width = 20,
                Height = 20
            }
        });

        AddonsMenuItem.IsVisible = hasAnyMenu;
    }
}