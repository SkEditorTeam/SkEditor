using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using SkEditor.Controls;
using SkEditor.Utilities;
using SkEditor.Utilities.Styling;

namespace SkEditor.Views.Generators.Gui;

public partial class ExtendedItemSelector : AppWindow
{
    private readonly Item _item;

    private MenuFlyout contextFlyout;

    public ExtendedItemSelector(Item item)
    {
        InitializeComponent();
        Focusable = true;

        _item = item;

        CheckForEditing();

        WindowStyler.Style(this);
        TitleBar.ExtendsContentIntoTitleBar = false;

        AssignCommands(item);
        SetContextMenu();
    }

    private void AssignCommands(Item item)
    {
        ContinueButton.Command = new RelayCommand(() =>
        {
            _item.Lore = [];
            LoreLineStackPanel.Children
                .OfType<LoreLineEditor>()
                .Where(x => !string.IsNullOrWhiteSpace(x.LineTextBox.Text))
                .ToList()
                .ForEach(x => _item.Lore.Add(x.LineTextBox.Text));

            if (!string.IsNullOrWhiteSpace(DisplayNameTextBox.Text))
            {
                item.HaveCustomName = true;
                item.CustomName = DisplayNameTextBox.Text;
            }

            if (!string.IsNullOrWhiteSpace(CustomModelDataTextBox.Text))
            {
                bool isInt = int.TryParse(CustomModelDataTextBox.Text, out int result);
                if (isInt)
                {
                    item.HaveCustomModelData = true;
                    item.CustomModelData = result;
                }
            }

            item.HaveExampleAction = ExampleActionCheckBox.IsChecked == true;

            Close(_item);
        });

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };

        ColoredTextHandler.SetupBox(DisplayNameTextBox);
    }

    private void CheckForEditing()
    {
        Item editedItem = ItemContextMenu.EditedItem;
        if (editedItem == null)
        {
            return;
        }

        if (editedItem.HaveCustomName)
        {
            DisplayNameTextBox.Text = editedItem.CustomName;
        }

        if (editedItem.HaveCustomModelData)
        {
            CustomModelDataTextBox.Text = editedItem.CustomModelData.ToString();
        }

        if (editedItem.Lore.Count > 0)
        {
            FirstLoreLine.LineTextBox.Text = editedItem.Lore[0];

            for (int i = 1; i < editedItem.Lore.Count; i++)
            {
                LoreLineEditor lineEditor = new()
                {
                    IsDeleteButtonVisible = true
                };
                lineEditor.LineTextBox.ContextFlyout = contextFlyout;
                lineEditor.DeleteButton.Command =
                    new RelayCommand(() => LoreLineStackPanel.Children.Remove(lineEditor));
                lineEditor.LineTextBox.Text = editedItem.Lore[i];
                LoreLineStackPanel.Children.Add(lineEditor);
            }
        }
    }

    private void SetContextMenu()
    {
        MenuItem addMenuItem = new()
        {
            Header = Translation.Get("GuiGeneratorAddLoreLine"),
            Icon = new SymbolIcon { Symbol = Symbol.Add, FontSize = 20 },
            Command = new RelayCommand(() =>
            {
                LoreLineEditor lineEditor = new()
                {
                    IsDeleteButtonVisible = true
                };
                lineEditor.LineTextBox.ContextFlyout = contextFlyout;
                lineEditor.DeleteButton.Command =
                    new RelayCommand(() => LoreLineStackPanel.Children.Remove(lineEditor));
                LoreLineStackPanel.Children.Add(lineEditor);
            })
        };

        MenuFlyout menuFlyout = new()
        {
            Items = { addMenuItem }
        };

        contextFlyout = menuFlyout;

        FirstLoreLine.LineTextBox.ContextFlyout = menuFlyout;
        FirstLoreLine.LineTextBox.Watermark = Translation.Get("GuiGeneratorLoreLineWatermark");
    }
}