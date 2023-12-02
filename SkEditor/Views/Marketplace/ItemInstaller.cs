using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Serilog;
using SkEditor.API;
using SkEditor.Controls;
using SkEditor.Utilities;
using SkEditor.Utilities.Styling;
using SkEditor.Utilities.Syntax;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace SkEditor.Views.Marketplace;
public class ItemInstaller
{
	private static MarketplaceItemView _itemView => MarketplaceWindow.Instance.ItemView;

	public static async void InstallItem(MarketplaceItem item)
	{
		if (item == null) return;

		bool isAddon = item.ItemType.Equals("Addon");

		string fileName = item.ItemFileUrl.Split('/').Last();
		string folderName = GetFolder(item.ItemType);
		string filePath = Path.Combine(AppConfig.AppDataFolderPath, folderName, fileName);

		using HttpClient client = new();
		HttpResponseMessage response = await client.GetAsync(item.ItemFileUrl);
		try
		{
			using Stream stream = await response.Content.ReadAsStreamAsync();
			using FileStream fileStream = File.Create(filePath);
			await stream.CopyToAsync(fileStream);
			await stream.DisposeAsync();

			string message = Translation.Get("MarketplaceInstallSuccess", item.ItemName);

			if (item.ItemRequiresRestart)
			{
				message += "\n" + Translation.Get("MarketplaceInstallRestart");
			}
			else if (item.ItemType.Equals("Syntax highlighting") || item.ItemType.Equals("Theme"))
			{
				message += "\n" + Translation.Get("MarketplaceInstallEnableNow");
			}
			else
			{
				message += "\n" + Translation.Get("MarketplaceInstallNoNeedToRestart");
			}

			ContentDialogResult result = await ApiVault.Get().ShowMessageWithIcon("Success", message,
				new SymbolIconSource() { Symbol = Symbol.Accept }, primaryButton: !isAddon,
				primaryButtonContent: "MarketplaceEnableNow", closeButtonContent: "Okay");

			if (result == ContentDialogResult.Primary)
			{
				if (item.ItemType.Equals("Syntax highlighting"))
				{
					SyntaxLoader.Syntaxes.Add(fileName);
					ApiVault.Get().GetAppConfig().CurrentSyntax = fileName;
					_ = Dispatcher.UIThread.InvokeAsync(() => SyntaxLoader.UpdateSyntax(SyntaxLoader.SyntaxFilePath));
				}
				else if (item.ItemType.Equals("Theme"))
				{
					_ = Dispatcher.UIThread.InvokeAsync(() =>
					{
						Theme theme = ThemeEditor.LoadTheme(filePath);
						ThemeEditor.SetTheme(theme);
					});
				}
			}

			if (isAddon)
			{
				MarketplaceWindow.Instance.HideAllButtons();
				_itemView.UninstallButton.IsVisible = true;
				_itemView.DisableButton.IsVisible = true;
				EnableAddon(item);
			}
			else if (item.ItemType.Equals("Syntax highlighting") || item.ItemType.Equals("Theme"))
			{
				MarketplaceWindow.Instance.HideAllButtons();
				_itemView.UninstallButton.IsVisible = true;
			}
		}
		catch (Exception e)
		{
			Log.Error(e, "Failed to install addon!");
			ApiVault.Get().ShowMessage(Translation.Get("Error"), Translation.Get("MarketplaceInstallFailed", item.ItemName));
		}
	}

	private static void EnableAddon(MarketplaceItem item)
	{
		if (item.ItemRequiresRestart) return;

		string fileName = item.ItemFileUrl.Split('/').Last();
		string filePath = Path.Combine(AppConfig.AppDataFolderPath, "Addons", fileName);

		_ = Dispatcher.UIThread.InvokeAsync(async () =>
		{

			Assembly assembly = Assembly.LoadFrom(filePath);
			IAddon addon = assembly.GetTypes().FirstOrDefault(p => typeof(IAddon).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract) is Type addonType
				? (IAddon)Activator.CreateInstance(addonType)
				: null;

			if (addon != null)
			{
				AddonLoader.Addons.Add(addon);
				addon.OnEnable();
			}
			else
			{
				Log.Error($"Failed to enable addon '{item.ItemName}'!");
			}
		});
	}

	public static string GetFolder(string type)
	{
		return type switch
		{
			"Addon" => "Addons",
			"Theme" => "Themes",
			"Syntax Highlighting" => "Syntax Highlighting",
			_ => ""
		};
	}

	public static async void UninstallItem(MarketplaceItem item)
	{
		string fileName = item.ItemFileUrl.Split('/').Last();

		if (item.ItemType.Equals("Syntax highlighting"))
		{
			SyntaxLoader.Syntaxes.Remove(fileName);
			ApiVault.Get().GetAppConfig().CurrentSyntax = null;
			_ = Dispatcher.UIThread.InvokeAsync(() => SyntaxLoader.UpdateSyntax(SyntaxLoader.SyntaxFilePath));
			File.Delete(Path.Combine(AppConfig.AppDataFolderPath, "Syntax Highlighting", fileName));

			MarketplaceWindow.Instance.HideAllButtons();
			_itemView.InstallButton.IsVisible = true;

			return;
		}
		else if (item.ItemType.Equals("Theme"))
		{
			if (fileName.Equals(ThemeEditor.CurrentTheme.FileName)) 
				ThemeEditor.SetTheme(ThemeEditor.Themes.FirstOrDefault(x => x.FileName.Equals("Default.json")) ?? ThemeEditor.GetDefaultTheme());

			ThemeEditor.Themes.Remove(ThemeEditor.Themes.FirstOrDefault(x => x.FileName.Equals(fileName)));
			ThemeEditor.SaveAllThemes();
			File.Delete(Path.Combine(AppConfig.AppDataFolderPath, "Themes", fileName));

			MarketplaceWindow.Instance.HideAllButtons();
			_itemView.InstallButton.IsVisible = true;

			return;
		}

		ApiVault.Get().GetAppConfig().AddonsToDelete.Add(fileName);
		ApiVault.Get().GetAppConfig().Save();

		MarketplaceWindow.Instance.HideAllButtons();
		_itemView.InstallButton.IsVisible = true;

		await ApiVault.Get().ShowMessageWithIcon(Translation.Get("Success"), Translation.Get("MarketplaceUninstallSuccess"), new SymbolIconSource() { Symbol = Symbol.Accept }, primaryButton: false, closeButtonContent: "Okay");
	}

	public static void DisableItem(MarketplaceItem item)
	{
		string fileName = item.ItemFileUrl.Split('/').Last();
		ApiVault.Get().GetAppConfig().AddonsToDisable.Add(fileName);
		ApiVault.Get().GetAppConfig().Save();
		_itemView.DisableButton.IsVisible = false;
		_itemView.EnableButton.IsVisible = true;
	}

	public static void EnableItem(MarketplaceItem item)
	{
		string fileName = item.ItemFileUrl.Split('/').Last();
		ApiVault.Get().GetAppConfig().AddonsToDisable.Remove(fileName);
		ApiVault.Get().GetAppConfig().Save();
		_itemView.DisableButton.IsVisible = true;
		_itemView.EnableButton.IsVisible = false;
	}

	public static async void UpdateItem(MarketplaceItem item)
	{
		string fileName = "updated-" + item.ItemFileUrl.Split('/').Last();
		ApiVault.Get().GetAppConfig().AddonsToUpdate.Add(fileName);
		ApiVault.Get().GetAppConfig().Save();
		_itemView.UpdateButton.IsEnabled = false;

		string filePath = Path.Combine(AppConfig.AppDataFolderPath, "Addons", fileName);

		using HttpClient client = new();
		HttpResponseMessage response = await client.GetAsync(item.ItemFileUrl);

		try
		{
			using Stream stream = await response.Content.ReadAsStreamAsync();
			using FileStream fileStream = File.Create(filePath);
			await stream.CopyToAsync(fileStream);

			ContentDialogResult result = await ApiVault.Get().ShowMessageWithIcon(Translation.Get("Success"), Translation.Get("MarketplaceUpdateSuccess", item.ItemName),
				new SymbolIconSource() { Symbol = Symbol.Accept }, primaryButton: false, closeButtonContent: "Okay");
		}
		catch (Exception e)
		{
			ApiVault.Get().ShowMessage(Translation.Get("Error"), Translation.Get("MarketplaceUpdateFailed", item.ItemName));
		}
	}
}
