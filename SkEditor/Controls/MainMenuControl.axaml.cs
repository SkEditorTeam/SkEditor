using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.Utilities.Editor;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Syntax;
using SkEditor.Views;
using SkEditor.Views.Generators;
using SkEditor.Views.Generators.Gui;
using System;

namespace SkEditor.Controls;
public partial class MainMenuControl : UserControl
{

    public MainMenuControl()
    {
        InitializeComponent();

        AssignCommands();
    }

    private void AssignCommands()
    {
        MenuItemNew.Command = new RelayCommand(FileHandler.NewFile);
        MenuItemOpen.Command = new RelayCommand(FileHandler.OpenFile);
        MenuItemSave.Command = new RelayCommand(async () =>
        {
            (bool, Exception) success = await FileHandler.SaveFile();
            if (!success.Item1)
            {
                ApiVault.Get().ShowError("For some reason, the file couldn't be saved. If the problem persists, backup the file so you won't lose any changes.\nError: " + success.Item2.Message);
            }
        });
        MenuItemSaveAs.Command = new RelayCommand(FileHandler.SaveAsFile);
        MenuItemPublish.Command = new RelayCommand(() => new PublishWindow().ShowDialog(ApiVault.Get().GetMainWindow()));
        MenuItemClose.Command = new RelayCommand(FileHandler.CloseCurrentFile);
        MenuItemCloseAll.Command = new RelayCommand(FileHandler.CloseAllFiles);

        MenuItemCopy.Command = new RelayCommand(() => ApiVault.Get().GetTextEditor().Copy());
        MenuItemPaste.Command = new RelayCommand(() => ApiVault.Get().GetTextEditor().Paste());
        MenuItemCut.Command = new RelayCommand(() => ApiVault.Get().GetTextEditor().Cut());
        MenuItemUndo.Command = new RelayCommand(() => ApiVault.Get().GetTextEditor().Undo());
        MenuItemRedo.Command = new RelayCommand(() => ApiVault.Get().GetTextEditor().Redo());
        MenuItemDelete.Command = new RelayCommand(() => ApiVault.Get().GetTextEditor().Delete());

        MenuItemDuplicate.Command = new RelayCommand(() => CustomCommandsHandler.OnDuplicateCommandExecuted(ApiVault.Get().GetTextEditor().TextArea));
        MenuItemComment.Command = new RelayCommand(() => CustomCommandsHandler.OnCommentCommandExecuted(ApiVault.Get().GetTextEditor().TextArea));

        MenuItemRefreshSyntax.Command = new RelayCommand(SyntaxLoader.RefreshSyntax);

        MenuItemSettings.Command = new RelayCommand(() => new SettingsWindow().ShowDialog(ApiVault.Get().GetMainWindow()));
        MenuItemGenerateGui.Command = new RelayCommand(() => new GuiGenerator().ShowDialog(ApiVault.Get().GetMainWindow()));
        MenuItemGenerateCommand.Command = new RelayCommand(() => new CommandGenerator().ShowDialog(ApiVault.Get().GetMainWindow()));
        MenuItemRefactor.Command = new RelayCommand(() => new RefactorWindow().ShowDialog(ApiVault.Get().GetMainWindow()));
        MenuItemMarketplace.Command = new RelayCommand(() => new MarketplaceWindow().ShowDialog(ApiVault.Get().GetMainWindow()));
    }
}
