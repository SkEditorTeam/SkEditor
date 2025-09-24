using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using Newtonsoft.Json;
using SkEditor.API;
using SkEditor.Data;
using SkEditor.Utilities;
using SkEditor.Utilities.Styling;

namespace SkEditor.Views.Windows.Generators.Gui;

public partial class ItemSelector : AppWindow
{
    private readonly ItemBindings _itemBindings = new();

    public ItemSelector()
    {
        InitializeComponent();

        DataContext = _itemBindings;

        WindowStyler.Style(this);
        TitleBar.ExtendsContentIntoTitleBar = false;

        SelectButton.Command = new RelayCommand(() =>
        {
            ComboBoxItem? comboBoxItem = (ComboBoxItem?)ItemListBox.SelectedItem;
            if (comboBoxItem == null)
            {
                Close();
                return;
            }

            Item item = _itemBindings.Items.First(x => x.Name.Equals(comboBoxItem.Tag?.ToString()));
            Close(item);
        });
        CancelButton.Command = new RelayCommand(Close);

        SearchBox.TextChanged += OnSearchChanged;

        Loaded += OnItemSelectorLoaded;

        KeyDown += (_, e) =>
        {
            switch (e.Key)
            {
                case Key.Enter:
                    SelectButton.Command.Execute(null);
                    break;
                case Key.Escape:
                    Close();
                    break;
            }
        };
    }

    private async void OnItemSelectorLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            await CheckForFile();
            CheckForEditing();
            SearchBox.Focus();
            SearchBox.CaretIndex = SearchBox.Text?.Length ?? 0;
        }
        catch (Exception exc)
        {
            SkEditorAPI.Logs.Error("Error loading items: " + exc.Message);
        }
    }

    private void CheckForEditing()
    {
        if (ItemContextMenu.EditedItem == null)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                UpdateFilteredItems(null);
            }
            return;
        }

        SearchBox.Text = ItemContextMenu.EditedItem.DisplayName;
        
        UpdateFilteredItems(SearchBox.Text);
    }

    private void OnSearchChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateFilteredItems(SearchBox.Text);
    }

    private void UpdateFilteredItems(string? searchText)
    {
        ObservableCollection<Item> itemsToFilter = _itemBindings.Items;
        List<Item> filteredItemsResult;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            filteredItemsResult = new List<Item>(itemsToFilter);
        }
        else
        {
            filteredItemsResult = itemsToFilter
                .Where(x => x.DisplayName.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var exactMatch = filteredItemsResult.FirstOrDefault(item =>
                item.DisplayName.ToString().Equals(searchText, StringComparison.OrdinalIgnoreCase));

            if (exactMatch != null)
            {
                filteredItemsResult.Remove(exactMatch);
                filteredItemsResult.Insert(0, exactMatch);
            }
        }

        _itemBindings.FilteredItems = new ObservableCollection<ComboBoxItem>(
            filteredItemsResult.Select(CreateItem)
        );

        if (_itemBindings.FilteredItems.Any())
        {
            ItemListBox.SelectedIndex = 0;
        }
        else
        {
            ItemListBox.SelectedIndex = -1;
        }
    }

    private async Task CheckForFile()
    {
        string itemsFile = Path.Combine(AppConfig.AppDataFolderPath, "items.json");

        if (!File.Exists(itemsFile))
        {
            return;
        }

        try
        {
            List<Item>? items = JsonConvert.DeserializeObject<List<Item>>(await File.ReadAllTextAsync(itemsFile));
            if (items == null) return;

            _itemBindings.Items.Clear();
            _itemBindings.FilteredItems.Clear();

            foreach (Item item in items)
            {
                _itemBindings.Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading items.json: {ex.Message}");
        }
    }

    private static ComboBoxItem CreateItem(Item item)
    {
        return new ComboBoxItem
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Children =
                {
                    new Image
                    {
                        Source = item.Icon, 
                        Width = 24,
                        Height = 24
                    },
                    new TextBlock
                    {
                        Text = item.DisplayName,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            },
            Tag = item.Name
        };
    }
}

public class Item
{
    [JsonIgnore] private Bitmap _image = null!;

    [JsonProperty("name")] public required string Name { get; set; }

    [JsonProperty("displayName")] public required string DisplayName { get; set; }

    [JsonIgnore] public bool HaveCustomName { get; set; }

    [JsonIgnore] public string CustomName { get; set; } = string.Empty;

    [JsonIgnore] public List<string> Lore { get; set; } = [];

    [JsonIgnore] public bool HaveCustomModelData { get; set; }

    [JsonIgnore] public int CustomModelData { get; set; }

    [JsonIgnore] public bool HaveExampleAction { get; set; }

    [JsonIgnore]
    public Bitmap? Icon
    {
        get
        {
            if (_image != null!) return _image;
            if (GuiGenerator.Instance == null) return null;

            string itemImagePath = Path.Combine(GuiGenerator.Instance.ItemPath, Name + ".png");
            if (!File.Exists(itemImagePath))
            {
                itemImagePath = Path.Combine(GuiGenerator.Instance.ItemPath, "barrier.png");
            }

            _image = new Bitmap(itemImagePath);

            return _image;
        }
    }

    public async Task<Bitmap?> GetIcon()
    {
        return await Task.Run(() => Icon);
    }
}