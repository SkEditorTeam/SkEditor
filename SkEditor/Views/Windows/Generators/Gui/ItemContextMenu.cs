using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities;
using SymbolIcon = FluentIcons.Avalonia.Fluent.SymbolIcon;
using Symbol = FluentIcons.Common.Symbol;

namespace SkEditor.Views.Windows.Generators.Gui;

internal class ItemContextMenu
{
    private static Item? _copiedItem;
    public static Item? EditedItem { get; set; }

    public static MenuFlyout Get(int slot)
    {
        MenuItem editItem = CreateMenuItem("MenuHeaderEdit", Symbol.Edit, () => EditItem(slot));

        MenuItem copyItem = CreateMenuItem("MenuHeaderCopy", Symbol.Copy, () => CopyItem(slot));

        MenuItem pasteItem = CreateMenuItem("MenuHeaderPaste", Symbol.Clipboard, () => PasteItem(slot));

        MenuItem deleteItem = CreateMenuItem("MenuHeaderDelete", Symbol.Delete, () => DeleteItem(slot));

        return new MenuFlyout
        {
            Items = { editItem, copyItem, pasteItem, deleteItem }
        };
    }

    private static MenuItem CreateMenuItem(string headerKey, Symbol symbol, Func<Task> asyncAction)
    {
        return new MenuItem
        {
            Header = Translation.Get(headerKey),
            Icon = new SymbolIcon { Symbol = symbol, FontSize = 20 },
            Command = new AsyncRelayCommand(asyncAction)
        };
    }

    private static async Task EditItem(int slot)
    {
        GuiGenerator? generator = GuiGenerator.Instance;
        if (generator == null)
        {
            return;
        }

        EditedItem = slot == -1
            ? generator.BackgroundItem
            : generator.Items.GetValueOrDefault(slot);

        Item? item = await generator.SelectItem();
        if (item == null)
        {
            return;
        }

        if (slot == -1)
        {
            generator.BackgroundItem = item;
            generator.BackgroundItemButton.Content = item.DisplayName;
        }
        else
        {
            generator.Items[slot] = item;
            generator.UpdateItem(slot, item);
        }

        EditedItem = null;
    }

    private static Task CopyItem(int slot)
    {
        _copiedItem = slot == -1
            ? GuiGenerator.Instance?.BackgroundItem
            : GuiGenerator.Instance?.Items.GetValueOrDefault(slot);

        return Task.CompletedTask;
    }

    private static Task PasteItem(int slot)
    {
        if (GuiGenerator.Instance == null || _copiedItem == null)
        {
            return Task.CompletedTask;
        }

        if (slot == -1)
        {
            GuiGenerator.Instance.BackgroundItem = _copiedItem;
            GuiGenerator.Instance.BackgroundItemButton.Content = _copiedItem.DisplayName;
        }
        else
        {
            GuiGenerator.Instance.Items[slot] = _copiedItem;
            GuiGenerator.Instance.UpdateItem(slot, _copiedItem);
        }

        return Task.CompletedTask;
    }

    private static Task DeleteItem(int slot)
    {
        if (GuiGenerator.Instance == null)
        {
            return Task.CompletedTask;
        }

        if (slot == -1)
        {
            GuiGenerator.Instance.BackgroundItem = null;
            GuiGenerator.Instance.BackgroundItemButton.Content = Translation.Get("SelectButton");
        }
        else
        {
            GuiGenerator.Instance.Items.Remove(slot);
            Button? button = GuiGenerator.Instance.Buttons.FirstOrDefault(x => (int?)x.Tag == slot);
            if (button != null)
            {
                button.Content = "";
            }
        }

        return Task.CompletedTask;
    }
}