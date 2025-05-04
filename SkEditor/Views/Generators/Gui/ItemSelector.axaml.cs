using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using Newtonsoft.Json;
using SkEditor.Data;
using SkEditor.Utilities;
using SkEditor.Utilities.Styling;

namespace SkEditor.Views.Generators.Gui;

public partial class ItemSelector : AppWindow
{
    private readonly ItemBindings _itemBindings = new();

    public ItemSelector()
    {
        InitializeComponent();

        DataContext = _itemBindings;

        WindowStyler.Style(this);
        TitleBar.ExtendsContentIntoTitleBar = false;

        CheckForEditing();

        SelectButton.Command = new RelayCommand(() =>
        {
            ComboBoxItem comboBoxItem = (ComboBoxItem)ItemListBox.SelectedItem;
            if (comboBoxItem == null)
            {
                Close();
                return;
            }

            Item item = _itemBindings.Items.First(x => x.Name.Equals(comboBoxItem.Tag.ToString()));
            Close(item);
        });
        CancelButton.Command = new RelayCommand(Close);

        SearchBox.TextChanged += OnSearchChanged;

        Dispatcher.UIThread.InvokeAsync(CheckForFile);

        Loaded += (_, _) => { SearchBox.Focus(); };

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
                case Key.Up:
                    ItemListBox.SelectedIndex--;
                    break;
                case Key.Down:
                    ItemListBox.SelectedIndex++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        };
    }

    private void CheckForEditing()
    {
        if (ItemContextMenu.EditedItem == null)
        {
            return;
        }

        SearchBox.Text = ItemContextMenu.EditedItem.DisplayName;
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        string? searchText = SearchBox.Text;
        List<Item> filteredItems = _itemBindings.Items
            .Where(x => x.DisplayName.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (filteredItems.Any(item =>
                item.DisplayName.ToString().Equals(searchText, StringComparison.OrdinalIgnoreCase)))
        {
            Item item = filteredItems.First(item =>
                item.DisplayName.ToString().Equals(searchText, StringComparison.OrdinalIgnoreCase));
            filteredItems.Remove(item);
            filteredItems.Insert(0, item);
        }

        _itemBindings.FilteredItems = new ObservableCollection<ComboBoxItem>(
            filteredItems.Select(CreateItem)
        );

        ItemListBox.SelectedIndex = 0;
    }

    private async Task CheckForFile()
    {
        string itemsFile = Path.Combine(AppConfig.AppDataFolderPath, "items.json");

        if (!File.Exists(itemsFile))
        {
            return;
        }

        List<Item> items = JsonConvert.DeserializeObject<List<Item>>(await File.ReadAllTextAsync(itemsFile));

        foreach (Item item in items)
        {
            _itemBindings.Items.Add(item);
            _itemBindings.FilteredItems.Add(CreateItem(item));
        }
    }

    private ComboBoxItem CreateItem(Item item)
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

    [JsonIgnore] public string CustomName { get; set; }

    [JsonIgnore] public List<string> Lore { get; set; }

    [JsonIgnore] public bool HaveCustomModelData { get; set; }

    [JsonIgnore] public int CustomModelData { get; set; }

    [JsonIgnore] public bool HaveExampleAction { get; set; }

    [JsonIgnore]
    public Bitmap Icon
    {
        get
        {
            if (_image == null!)
            {
                string itemImagePath = Path.Combine(GuiGenerator.Instance.ItemPath, Name + ".png");
                if (!File.Exists(itemImagePath))
                {
                    itemImagePath = Path.Combine(GuiGenerator.Instance.ItemPath, "barrier.png");
                }

                _image = new Bitmap(itemImagePath);
            }

            return _image;
        }
    }

    public async Task<Bitmap> GetIcon()
    {
        return await Task.Run(() => Icon);
    }
}