using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Editor;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Projects;
using SkEditor.Utilities.Syntax;
using SkEditor.Views;
using SkEditor.Views.Generators;
using SkEditor.Views.Generators.Gui;
using System;
using System.Collections;
using System.Collections.Generic;
using SkEditor.Controls.Docs;

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
        MenuItemOpenFolder.Command = new RelayCommand(() => ProjectOpener.OpenProject());
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

        MenuItemClose.Command = new RelayCommand(FileCloser.CloseCurrentFile);
        MenuItemCloseAll.Command = new RelayCommand(FileCloser.CloseAllFiles);
        MenuItemCloseAllExceptCurrent.Command = new RelayCommand(FileCloser.CloseAllExceptCurrent);
        MenuItemCloseAllUnsaved.Command = new RelayCommand(FileCloser.CloseUnsaved);
        MenuItemCloseAllLeft.Command = new RelayCommand(FileCloser.CloseAllToTheLeft);
        MenuItemCloseAllRight.Command = new RelayCommand(FileCloser.CloseAllToTheRight);

        MenuItemCopy.Command = new RelayCommand(() => ApiVault.Get().GetTextEditor().Copy());
        MenuItemPaste.Command = new RelayCommand(() => ApiVault.Get().GetTextEditor().Paste());
        MenuItemCut.Command = new RelayCommand(() => ApiVault.Get().GetTextEditor().Cut());
        MenuItemUndo.Command = new RelayCommand(() => ApiVault.Get().GetTextEditor().Undo());
        MenuItemRedo.Command = new RelayCommand(() => ApiVault.Get().GetTextEditor().Redo());
        MenuItemDelete.Command = new RelayCommand(() => ApiVault.Get().GetTextEditor().Delete());

        MenuItemDuplicate.Command = new RelayCommand(() => CustomCommandsHandler.OnDuplicateCommandExecuted(ApiVault.Get().GetTextEditor().TextArea));
        MenuItemComment.Command = new RelayCommand(() => CustomCommandsHandler.OnCommentCommandExecuted(ApiVault.Get().GetTextEditor().TextArea));

        MenuItemRefreshSyntax.Command = new RelayCommand(async () => await SyntaxLoader.RefreshSyntaxAsync());

        MenuItemSettings.Command = new RelayCommand(() => new SettingsWindow().ShowDialog(ApiVault.Get().GetMainWindow()));
        MenuItemGenerateGui.Command = new RelayCommand(() => new GuiGenerator().ShowDialog(ApiVault.Get().GetMainWindow()));
        MenuItemGenerateCommand.Command = new RelayCommand(() => new CommandGenerator().ShowDialog(ApiVault.Get().GetMainWindow()));
        MenuItemRefactor.Command = new RelayCommand(() => new RefactorWindow().ShowDialog(ApiVault.Get().GetMainWindow()));
        MenuItemMarketplace.Command = new RelayCommand(() => new MarketplaceWindow().ShowDialog(ApiVault.Get().GetMainWindow()));
        
        MenuItemDocs.Command = new RelayCommand(AddDocsTab);
    }
    
    public void AddDocsTab()
    {
        var tabView = ApiVault.Get().GetTabView();
        var tabItem = new TabViewItem()
        {
            Header = "Documentation",
            Content = new DocumentationControl()
        };

        (tabView.TabItems as IList)?.Add(tabItem);
        tabView.SelectedItem = tabItem;
    }

    public void LoadAddonsMenus()
    {
        bool hasAnyMenu = false;
        foreach (IAddon addon in AddonLoader.Addons)
        {
            var items = addon.GetMenuItems();
            if (items.Count <= 0)
                continue;

            hasAnyMenu = true;
            var menuItem = new MenuItem()
            {
                Header = addon.Name,
                Icon = new SymbolIcon() { Symbol = addon.GetMenuIcon() }
            };

            foreach (MenuItem sub in items)
                menuItem.Items.Add(sub);

            AddonsMenuItem.Items.Add(menuItem);
        }

        AddonsMenuItem.IsVisible = hasAnyMenu;
    }
}
