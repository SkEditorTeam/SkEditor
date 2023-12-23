using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SkEditor.Views.Generators.Gui;
internal class ItemContextMenu
{
    private static Item copiedItem = null;
    public static Item EditedItem { get; set; }

    public static MenuFlyout Get(int slot)
    {
        MenuItem editItem = CreateMenuItem("MenuHeaderEdit", Symbol.Edit, async () => await EditItem(slot));

        MenuItem copyItem = CreateMenuItem("MenuHeaderCopy", Symbol.Copy, () => CopyItem(slot));

        MenuItem pasteItem = CreateMenuItem("MenuHeaderPaste", Symbol.Paste, () => PasteItem(slot));

        MenuItem deleteItem = CreateMenuItem("MenuHeaderDelete", Symbol.Delete, () => DeleteItem(slot));

        return new MenuFlyout()
        {
            Items = { editItem, copyItem, pasteItem, deleteItem }
        };
    }

    private static MenuItem CreateMenuItem(string headerKey, Symbol symbol, Action action)
    {
        return new MenuItem()
        {
            Header = Translation.Get(headerKey),
            Icon = new SymbolIcon() { Symbol = symbol, FontSize = 20 },
            Command = new RelayCommand(() => action.Invoke())
        };
    }

    private static async Task EditItem(int slot)
    {
        EditedItem = (slot == -1) ? GuiGenerator.Instance.BackgroundItem : GuiGenerator.Instance.Items.TryGetValue(slot, out Item? value) ? value : null;
        Item item = await GuiGenerator.Instance.SelectItem();
        if (item == null) return;
        if (slot == -1)
        {
            GuiGenerator.Instance.BackgroundItem = item;
            GuiGenerator.Instance.BackgroundItemButton.Content = item.DisplayName;
        }
        else
        {
            GuiGenerator.Instance.Items[slot] = item;
            GuiGenerator.Instance.UpdateItem(slot, item);
        }
        EditedItem = null;
    }

    private static void CopyItem(int slot)
    {
        copiedItem = (slot == -1) ? GuiGenerator.Instance.BackgroundItem : GuiGenerator.Instance.Items.TryGetValue(slot, out Item? value) ? value : null;
    }

    private static void PasteItem(int slot)
    {
        if (copiedItem == null) return;
        if (slot == -1)
        {
            GuiGenerator.Instance.BackgroundItem = copiedItem;
            GuiGenerator.Instance.BackgroundItemButton.Content = copiedItem.DisplayName;
        }
        else
        {
            GuiGenerator.Instance.Items[slot] = copiedItem;
            GuiGenerator.Instance.UpdateItem(slot, copiedItem);
        }
    }

    private static void DeleteItem(int slot)
    {
        if (slot == -1)
        {
            GuiGenerator.Instance.BackgroundItem = null;
            GuiGenerator.Instance.BackgroundItemButton.Content = Translation.Get("SelectButton");
        }
        else
        {
            GuiGenerator.Instance.Items.Remove(slot);
            Button? button = GuiGenerator.Instance.Buttons.FirstOrDefault(x => (int)x.Tag == slot);
            if (button != null) button.Content = "";
        }
    }
}