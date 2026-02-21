using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit.Editing;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Utilities.Editor;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Projects;
using SkEditor.Utilities.Styling;
using SkEditor.Utilities.Syntax;
using SkEditor.Views.Controls.Docs;
using SkEditor.Views.Pages;
using ColorSelectionWindow = SkEditor.Views.Windows.ColorSelectionWindow;
using CommandGenerator = SkEditor.Views.Windows.Generators.CommandGenerator;
using GoToLineWindow = SkEditor.Views.Windows.GoToLineWindow;
using GuiGenerator = SkEditor.Views.Windows.Generators.Gui.GuiGenerator;
using MainWindow = SkEditor.Views.Windows.MainWindow;
using MarketplaceWindow = SkEditor.Views.Windows.Marketplace.MarketplaceWindow;
using PublishWindow = SkEditor.Views.Windows.PublishWindow;
using RefactorWindow = SkEditor.Views.Windows.RefactorWindow;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Views.Controls;

public partial class MainMenuControl : UserControl
{
    public MainMenuControl()
    {
        InitializeComponent();

        AssignCommands();
        AddMissingHotkeys();
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
            new AsyncRelayCommand(async () => await new PublishWindow().ShowDialogOnMainWindow());

        MenuItemClose.Command = new AsyncRelayCommand(FileCloser.CloseCurrentFile);
        MenuItemCloseAll.Command = new AsyncRelayCommand(FileCloser.CloseAllFiles);
        MenuItemCloseAllExceptCurrent.Command = new AsyncRelayCommand(FileCloser.CloseAllExceptCurrent);
        MenuItemCloseAllUnsaved.Command = new AsyncRelayCommand(FileCloser.CloseUnsaved);
        MenuItemCloseAllLeft.Command = new AsyncRelayCommand(FileCloser.CloseAllToTheLeft);
        MenuItemCloseAllRight.Command = new AsyncRelayCommand(FileCloser.CloseAllToTheRight);

        MenuItemCopy.Command = new RelayCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.Copy());
        MenuItemPaste.Command = new RelayCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.Paste());
        MenuItemCut.Command = new RelayCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.Cut());
        MenuItemUndo.Command = new RelayCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.Undo());
        MenuItemRedo.Command = new RelayCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.Redo());
        MenuItemGoToLine.Command = new AsyncRelayCommand(() => ShowDialogIfEditorIsOpen(new GoToLineWindow()));
        MenuItemFind.Command = new RelayCommand(() =>
        {
            SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.SearchPanel.IsReplaceMode = false;
            SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.SearchPanel.Close();
            SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.SearchPanel.Open();
        });
        MenuItemReplace.Command = new RelayCommand(() =>
        {
            SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.SearchPanel.IsReplaceMode = true;
            SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.SearchPanel.Close();
            SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.SearchPanel.Open();
        });

        MenuItemTrimWhitespaces.Command = new RelayCommand(() =>
        {
            TextArea? textArea = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.TextArea;
            if (textArea == null)
            {
                return;
            }

            CustomCommandsHandler.OnTrimWhitespacesCommandExecuted(textArea);
        });

        MenuItemDuplicate.Command = new RelayCommand(() =>
        {
            TextArea? textArea = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.TextArea;
            if (textArea == null)
            {
                return;
            }

            CustomCommandsHandler.OnDuplicateCommandExecuted(textArea);
        });
        MenuItemComment.Command = new RelayCommand(() =>
        {
            TextArea? textArea = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.TextArea;
            if (textArea == null)
            {
                return;
            }

            CustomCommandsHandler.OnCommentCommandExecuted(textArea);
        });

        MenuItemRefreshSyntax.Command = new AsyncRelayCommand(async () => await SyntaxLoader.RefreshSyntaxAsync());
        MenuItemRefreshTheme.Command = new AsyncRelayCommand(async () => await ThemeEditor.ReloadCurrentTheme());

        MenuItemDocs.Command = new RelayCommand(AddDocsTab);
        MenuItemGenerateGui.Command = new AsyncRelayCommand(() => ShowDialogIfEditorIsOpen(new GuiGenerator(), false));
        MenuItemGenerateCommand.Command =
            new AsyncRelayCommand(() => ShowDialogIfEditorIsOpen(new CommandGenerator(), false));
        MenuItemRefactor.Command = new AsyncRelayCommand(() => ShowDialogIfEditorIsOpen(new RefactorWindow(), false));
        MenuItemColorSelector.Command =
            new AsyncRelayCommand(() => new ColorSelectionWindow().ShowDialogOnMainWindow());

        MenuItemMarketplace.Command =
            new AsyncRelayCommand(() => new MarketplaceWindow().ShowDialogOnMainWindow());

        MenuItemSettings.Command = new RelayCommand(AddSettingsTab);
    }

    private static async Task ShowDialogIfEditorIsOpen(AppWindow window, bool openAsDialog = true)
    {
        if (SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor == null)
        {
            return;
        }

        if (openAsDialog)
        {
            await window.ShowDialogOnMainWindow();
        }
        else
        {
            window.ShowOnMainWindow();
        }
    }

    private void AddMissingHotkeys()
    {
        Loaded += (_, _) =>
        {
            MainWindow? mainWindow = SkEditorAPI.Windows.GetMainWindow();
            if (mainWindow == null)
            {
                return;
            }

            mainWindow.KeyDown += (_, e) =>
            {
                if (e is not { PhysicalKey: PhysicalKey.S, KeyModifiers: (KeyModifiers.Control | KeyModifiers.Alt) }
                    || !string.IsNullOrEmpty(e.KeySymbol))
                {
                    return;
                }

                MenuItemSaveAll.Command?.Execute(null);
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
        SkEditorAPI.Files.AddCustomTab(Translation.GetObservable(TranslationKey.DocumentationWindowTitle),
            new DocumentationControl(),
            icon: icon);
    }

    public void AddSettingsTab()
    {
        TabViewItem? existingTab = SkEditorAPI.Files.GetOpenedTabs().FirstOrDefault(tab => tab.Content is SettingsPage);
        if (existingTab is not null)
        {
            SkEditorAPI.Windows.GetMainWindow()?.TabControl?.SelectedItem = existingTab;
            return;
        }
        
        SettingsPage page = new();

        SymbolIconSource icon = new() { Symbol = Symbol.Settings };
        SkEditorAPI.Files.AddCustomTab(Translation.GetObservable(TranslationKey.WindowTitleSettings), page, icon: icon);
    }
}