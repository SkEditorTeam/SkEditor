using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using Newtonsoft.Json;
using SkEditor.Data;
using SkEditor.Utilities;
using SkEditor.Utilities.Styling;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SkEditor.Views.Generators.Gui;
public partial class ItemSelector : AppWindow
{
	private ItemBindings _itemBindings = new();

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

		CheckForFile();

		Loaded += (sender, e) =>
		{
			SearchBox.Focus();
		};

		KeyDown += (sender, e) =>
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
			}
		};
	}

	private void CheckForEditing()
	{
		if (ItemContextMenu.EditedItem == null) return;

		SearchBox.Text = ItemContextMenu.EditedItem.DisplayName;
	}

	private void OnSearchChanged(object sender, TextChangedEventArgs e)
	{
		var searchText = SearchBox.Text;
		var filteredItems = _itemBindings.Items
			.Where(x => x.DisplayName.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase))
			.ToList();

		if (filteredItems.Any(item => item.DisplayName.ToString().Equals(searchText, StringComparison.OrdinalIgnoreCase)))
		{
			var item = filteredItems.First(item => item.DisplayName.ToString().Equals(searchText, StringComparison.OrdinalIgnoreCase));
			filteredItems.Remove(item);
			filteredItems.Insert(0, item);
		}

		_itemBindings.FilteredItems = new ObservableCollection<ComboBoxItem>(
			filteredItems.Select(x => new ComboBoxItem { Content = x.DisplayName, Tag = x.Name })
		);

		ItemListBox.SelectedIndex = 0;
	}

	private void CheckForFile()
	{
		string itemsFile = Path.Combine(AppConfig.AppDataFolderPath, "items.json");

		if (!File.Exists(itemsFile)) return;

		List<Item> items = JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText(itemsFile));

		foreach (Item item in items)
		{
			_itemBindings.Items.Add(item);
			ComboBoxItem comboBoxItem = new()
			{
				Content = item.DisplayName,
				Tag = item.Name
			};
			_itemBindings.FilteredItems.Add(comboBoxItem);
		}
	}
}

public class Item
{
	[JsonProperty("name")]
	public required string Name { get; set; }

	[JsonProperty("displayName")]
	public required string DisplayName { get; set; }

	[JsonIgnore]
	public bool HaveCustomName { get; set; }
	[JsonIgnore]
	public string CustomName { get; set; }
	[JsonIgnore]
	public List<string> Lore { get; set; }
	[JsonIgnore]
	public bool HaveCustomModelData { get; set; }
	[JsonIgnore]
	public int CustomModelData { get; set; }
	[JsonIgnore]
	public bool HaveExampleAction { get; set; }
}