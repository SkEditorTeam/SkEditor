using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using Serilog;
using SkEditor.API;
using SkEditor.Controls;
using SkEditor.Utilities;
using SkEditor.Views.Marketplace;
using System;
using System.IO;
using System.Linq;

namespace SkEditor.Views;
public partial class MarketplaceWindow : AppWindow
{
	public const string MarketplaceUrl = "https://marketplace-skeditor.vercel.app";

	public static MarketplaceWindow Instance { get; private set; }

	public MarketplaceWindow()
	{
		InitializeComponent();

		Instance = this;

		ItemListBox.SelectionChanged += OnSelectedItemChanged;

		LoadItems();
	}

	private async void LoadItems()
	{
		try
		{
			await foreach (MarketplaceItem item in MarketplaceLoader.GetItems())
			{
				MarketplaceListItem listItem = new()
				{
					ItemName = item.ItemName,
					ItemShortDescription = item.ItemShortDescription,
					ItemImageUrl = item.ItemImageUrl,
					Tag = item
				};
				ItemListBox.Items.Add(listItem);
			}
		}
		catch (Exception e)
		{
			Log.Error(e, "Failed to load Marketplace items");
			ApiVault.Get().ShowMessage(Translation.Get("Error"), Translation.Get("MarketplaceLoadFailed"));
		}
	}

	private void OnSelectedItemChanged(object? sender, SelectionChangedEventArgs e)
	{
		ItemView.IsVisible = false;
		MarketplaceListItem listItem = (MarketplaceListItem)ItemListBox.SelectedItem;
		if (listItem == null) return;
		MarketplaceItem item = (MarketplaceItem)listItem.Tag;
		if (item == null) return;


		if (item.ItemType.Equals("Addon"))
		{
			IAddon addon = AddonLoader.Addons.FirstOrDefault(x => x.Name.Equals(item.ItemName));
			HideAllButtons();
			if (addon != null)
			{
				ItemView.UninstallButton.IsVisible = true;
				ItemView.DisableButton.IsVisible = true;
				ItemView.UpdateButton.IsVisible = !addon.Version.Equals(item.ItemVersion);
			}
			else if (ApiVault.Get().GetAppConfig().AddonsToDisable.Contains(Path.GetFileName(item.ItemFileUrl)))
			{
				ItemView.UninstallButton.IsVisible = true;
				ItemView.EnableButton.IsVisible = true;
			}
			else
			{
				ItemView.InstallButton.IsVisible = true;
			}
		}
		else if (item.ItemType.Equals("Syntax highlighting") || item.ItemType.Equals("Theme"))
		{
			HideAllButtons();
			if (File.Exists(Path.Combine(AppConfig.AppDataFolderPath, ItemInstaller.GetFolder(item.ItemType), Path.GetFileName(item.ItemFileUrl))))
			{
				ItemView.UninstallButton.IsVisible = true;
			}
			else
			{
				ItemView.InstallButton.IsVisible = true;
			}
		}

		ItemView.IsVisible = true;

		ItemView.ItemName = item.ItemName;
		ItemView.ItemVersion = item.ItemVersion;
		ItemView.ItemAuthor = item.ItemAuthor;
		ItemView.ItemShortDescription = item.ItemShortDescription;
		ItemView.ItemLongDescription = item.ItemLongDescription;
		ItemView.ItemImageUrl = item.ItemImageUrl;

		ItemView.InstallButton.CommandParameter = ItemView.DisableButton.CommandParameter = ItemView.UninstallButton.CommandParameter
				= ItemView.EnableButton.CommandParameter = ItemView.UpdateButton.CommandParameter = item;

		ItemView.InstallButton.Command = new RelayCommand<MarketplaceItem>(ItemInstaller.InstallItem);
		ItemView.UninstallButton.Command = new RelayCommand<MarketplaceItem>(ItemInstaller.UninstallItem);
		ItemView.DisableButton.Command = new RelayCommand<MarketplaceItem>(ItemInstaller.DisableItem);
		ItemView.EnableButton.Command = new RelayCommand<MarketplaceItem>(ItemInstaller.EnableItem);
		ItemView.UpdateButton.Command = new RelayCommand<MarketplaceItem>(ItemInstaller.UpdateItem);
	}

	public void HideAllButtons()
	{
		ItemView.InstallButton.IsVisible = false;
		ItemView.UninstallButton.IsVisible = false;
		ItemView.DisableButton.IsVisible = false;
		ItemView.EnableButton.IsVisible = false;
		ItemView.UpdateButton.IsVisible = false;
	}
}
