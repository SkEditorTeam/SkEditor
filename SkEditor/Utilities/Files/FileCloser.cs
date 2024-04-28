using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkEditor.Utilities.Files;
internal class FileCloser
{
    public static async void CloseFile(TabViewTabCloseRequestedEventArgs e) => await CloseFile(e.Tab);
    public static async void CloseCurrentFile() => await CloseFile(ApiVault.Get().GetTabView().SelectedItem as TabViewItem);

    public static async void CloseAllFiles()
    {
        if (await ShowConfirmationDialog() != ContentDialogResult.Primary) return;

        List<TabViewItem> tabItems = GetTabItems();
        tabItems.ForEach(DisposeEditorData);
        tabItems.ForEach(tabItem => RemoveTabItem(tabItem));
        FileHandler.NewFile();
    }

    public static async Task CloseFile(TabViewItem item, bool force = false)
    {
        if (item.Content is TextEditor editor && !ApiVault.Get().OnFileClosing(editor)) return;

        DisposeEditorData(item);

        string header = item.Header.ToString();

        if (header.EndsWith('*') && !force && await ShowConfirmationDialog() != ContentDialogResult.Primary) return;

        RemoveTabItem(item);
        FileHandler.OpenedFiles.RemoveAll(openedFile => openedFile.TabViewItem == item);
        FileBuilder.OpenedFiles.Remove(header);
        FileHandler.TabSwitchAction();

        if (GetTabItems().Count == 0) FileHandler.NewFile();
    }

    public static async void CloseAllExceptCurrent()
    {
        if (await ShowConfirmationDialog() != ContentDialogResult.Primary) return;

        TabViewItem currentTab = ApiVault.Get().GetTabView().SelectedItem as TabViewItem;
        List<TabViewItem> tabItems = GetTabItems();
        tabItems.Remove(currentTab);
        tabItems.ForEach(DisposeEditorData);
        tabItems.ForEach(RemoveTabItem);
    }

    public static async void CloseUnsaved()
    {
        if (await ShowConfirmationDialog() != ContentDialogResult.Primary) return;

        List<TabViewItem> tabItems = GetTabItems();
        tabItems.ForEach(async tabItem =>
        {
            if (tabItem.Header.ToString().EndsWith('*')) await CloseFile(tabItem, true);
        });
    }

    public static async void CloseAllToTheLeft()
    {
        if (await ShowConfirmationDialog() != ContentDialogResult.Primary) return;

        TabViewItem currentTab = ApiVault.Get().GetTabView().SelectedItem as TabViewItem;
        List<TabViewItem> tabItems = GetTabItems();
        int currentIndex = tabItems.IndexOf(currentTab);
        tabItems.GetRange(0, currentIndex).ForEach(async tabItem => await CloseFile(tabItem, true));

        ApiVault.Get().GetTabView().SelectedItem = currentTab;
    }

    public static async void CloseAllToTheRight()
    {
        if (await ShowConfirmationDialog() != ContentDialogResult.Primary) return;

        TabViewItem currentTab = ApiVault.Get().GetTabView().SelectedItem as TabViewItem;
        List<TabViewItem> tabItems = GetTabItems();
        int currentIndex = tabItems.IndexOf(currentTab);
        tabItems.GetRange(currentIndex + 1, tabItems.Count - currentIndex - 1).ForEach(async tabItem => await CloseFile(tabItem, true));
    }

    private static void DisposeEditorData(TabViewItem item)
    {
        if (item.Content is TextEditor editor)
            TextEditorEventHandler.ScrollViewers.Remove(editor);
    }

    private static void RemoveTabItem(TabViewItem item)
    {
        var tabView = ApiVault.Get().GetTabView();
        var tabItems = tabView.TabItems as IList;
        tabItems?.Remove(item);
    }

    private static List<TabViewItem> GetTabItems() => 
        (ApiVault.Get().GetTabView().TabItems as IList)?.Cast<TabViewItem>().ToList();

    private static async Task<ContentDialogResult> ShowConfirmationDialog() => 
        await ApiVault.Get().ShowMessageWithIcon(Translation.Get("Attention"),
            Translation.Get("ClosingFiles"), new SymbolIconSource { Symbol = Symbol.ImportantFilled });
}