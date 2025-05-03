using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Styling;
using SkEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SkEditor.Views.Generators.Gui;

public partial class GuiGenerator : AppWindow
{
    private IRelayCommand<int> _buttonCommand;

    public HashSet<Button> Buttons { get; } = [];
    public Dictionary<int, Item> Items { get; set; } = [];
    public Item? BackgroundItem { get; set; }
    public int CurrentRows { get; set; } = 6;


    public readonly string ItemPath = Path.Combine(AppConfig.AppDataFolderPath, "Items");
    public static GuiGenerator Instance { get; private set; }

    public GuiGenerator()
    {
        InitializeComponent();
        Focusable = true;

        Instance = this;
        DataContext = new SettingsViewModel();

        WindowStyler.Style(this);
        TitleBar.ExtendsContentIntoTitleBar = false;

        RowQuantityTextBox.TextChanged += (_, _) => UpdateRows();

        AssignCommands();

        BackgroundItemButton.ContextFlyout = ItemContextMenu.Get(-1);

        CreateGrid(6);

        Loaded += async (_, _) =>
        {
            await FileDownloader.CheckForMissingItemFiles(this);
            TitleTextBox.Focus();
        };

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape) Close();
        };
    }

    public async Task<Item> SelectItem()
    {
        ItemSelector itemSelector = new();
        Item item = await itemSelector.ShowDialog<Item>(this);
        if (item == null) return null;

        ExtendedItemSelector extendedItemSelector = new(item);
        Item extendedItem = await extendedItemSelector.ShowDialog<Item>(this);
        return extendedItem;
    }

    private void AssignCommands()
    {
        _buttonCommand = new AsyncRelayCommand<int>(async (slotId) =>
        {
            Item item = await SelectItem();
            if (item == null) return;

            UpdateItem(slotId, item);
        });
        BackgroundItemButton.Command = new AsyncRelayCommand(async () =>
        {
            Item item = await SelectItem();
            if (item == null) return;

            BackgroundItem = item;
            BackgroundItemButton.Content = item.DisplayName;
        });

        PreviewButton.Command = new RelayCommand(Preview.Show);
        GenerateButton.Command = new RelayCommand(Generation.Generate);
        UseSkriptGuiCheckBox.IsCheckedChanged += (_, _) =>
            SkEditorAPI.Core.GetAppConfig().UseSkriptGui = UseSkriptGuiCheckBox.IsChecked == true;
    }

    private void UpdateRows()
    {
        if (!int.TryParse(RowQuantityTextBox.Text, out int rows)) return;

        rows = Math.Clamp(rows, 1, 6);
        RowQuantityTextBox.Text = rows.ToString();
        CurrentRows = rows;

        ItemGrid.Children.Clear();
        Buttons.Clear();
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                Button button = CreateButton(row * 9 + column);
                Grid.SetRow(button, row + 1);
                Grid.SetColumn(button, column);
                ItemGrid.Children.Add(button);
                Buttons.Add(button);
            }
        }

        Items = Items.Where(x => x.Key < rows * 9).ToDictionary(x => x.Key, x => x.Value);
        foreach (KeyValuePair<int, Item> item in Items)
        {
            UpdateItem(item.Key, item.Value);
        }
    }


    public void UpdateItem(int slotId, Item item)
    {
        Button button = Buttons.FirstOrDefault(x => (int?)x.Tag == slotId);

        string itemImagePath = Path.Combine(ItemPath, item.Name + ".png");

        if (File.Exists(itemImagePath))
        {
            button.Content = new Image
            {
                Source = new Bitmap(itemImagePath),
                Width = 32,
                Height = 32,
                Stretch = Stretch.Uniform,
            };
        }
        else
        {
            button.Content = item.Name;
        }

        Items[slotId] = item;
    }

    private void CreateGrid(int rows)
    {
        int columns = 9;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Button button = CreateButton(row * columns + column);
                Grid.SetRow(button, row + 1);
                Grid.SetColumn(button, column);
                ItemGrid.Children.Add(button);
            }
        }
    }

    private Button CreateButton(int slotId)
    {
        Button button = new()
        {
            Content = "",
            Width = 48,
            Height = 48,
            Margin = new Thickness(3),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Tag = slotId,
            Command = _buttonCommand,
            CommandParameter = slotId,
            ContextFlyout = ItemContextMenu.Get(slotId)
        };

        Buttons.Add(button);

        return button;
    }
}