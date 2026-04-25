using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Styling;

namespace SkEditor.Views.Windows.Generators.Gui;

public partial class ItemSelector : FAAppWindow
{
    public ItemSelector()
    {
        InitializeComponent();

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

            Item item = Items.First(x => x.Name.Equals(comboBoxItem.Tag?.ToString()));
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

    public ObservableCollection<ComboBoxItem> FilteredItems { get; } = [];
    public ObservableCollection<Item> Items { get; } = [];

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
            App.Services.GetRequiredService<ILogService>().Error("Error loading items: " + exc.Message);
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
        ObservableCollection<Item> itemsToFilter = Items;
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

            Item? exactMatch = filteredItemsResult.FirstOrDefault(item =>
                item.DisplayName.ToString().Equals(searchText, StringComparison.OrdinalIgnoreCase));

            if (exactMatch != null)
            {
                filteredItemsResult.Remove(exactMatch);
                filteredItemsResult.Insert(0, exactMatch);
            }
        }

        FilteredItems.Clear();
        FilteredItems.AddRange(filteredItemsResult.Select(CreateItem));

        if (FilteredItems.Any())
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
        // string itemsFile = Path.Combine(SkEditorAPI.Configs.GetAppDataPath(), "items.json");
        string itemsFile = string.Empty;

        if (!File.Exists(itemsFile))
        {
            return;
        }

        try
        {
            List<Item>? items = JsonSerializer.Deserialize<List<Item>>(await File.ReadAllTextAsync(itemsFile),
                ItemJsonContext.Default.ListItem);

            if (items == null)
            {
                return;
            }

            Items.Clear();
            FilteredItems.Clear();

            Items.AddRange(items);
        }
        catch (Exception ex)
        {
            App.Services.GetRequiredService<ILogService>().Error($"Error loading items.json: {ex.Message}");
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

[JsonSerializable(typeof(Item))]
[JsonSerializable(typeof(List<Item>))]
public partial class ItemJsonContext : JsonSerializerContext;

public class Item
{
    [JsonIgnore]
    private Bitmap _image = null!;

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("displayName")]
    public required string DisplayName { get; set; }

    [JsonIgnore]
    public bool HaveCustomName { get; set; }

    [JsonIgnore]
    public string CustomName { get; set; } = string.Empty;

    [JsonIgnore]
    public List<string> Lore { get; set; } = [];

    [JsonIgnore]
    public bool HaveCustomModelData { get; set; }

    [JsonIgnore]
    public int CustomModelData { get; set; }

    [JsonIgnore]
    public bool HaveExampleAction { get; set; }

    [JsonIgnore]
    public Bitmap? Icon
    {
        get
        {
            if (_image != null!)
            {
                return _image;
            }

            if (GuiGenerator.Instance == null)
            {
                return null;
            }

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