using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkEditor.Views.Settings;
public partial class GeneralPage : UserControl
{
	public GeneralPage()
	{
		InitializeComponent();

		DataContext = new SettingsViewModel();

		AssignCommands();
		LoadLanguages();
	}

	private void LoadLanguages()
	{
		string[] files = Directory.GetFiles(Translation.LanguagesFolder);
		foreach (string file in files)
		{
			LanguageComboBox.Items.Add(Path.GetFileNameWithoutExtension(file));
		}
		LanguageComboBox.SelectedItem = ApiVault.Get().GetAppConfig().Language;
		LanguageComboBox.SelectionChanged += (s, e) =>
		{
			string language = LanguageComboBox.SelectedItem.ToString();
			ApiVault.Get().GetAppConfig().Language = language;
			Dispatcher.UIThread.InvokeAsync(() => Translation.ChangeLanguage(language));
		};
	}

	private void AssignCommands()
	{
		Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(HomePage)));
		RpcToggleSwitch.Command = new RelayCommand(ToggleRpc);
		WrappingToggleSwitch.Command = new RelayCommand(ToggleWrapping);
		AutoIndentToggleSwitch.Command = new RelayCommand(ToggleAutoIndent);
		AutoPairingToggleSwitch.Command = new RelayCommand(ToggleAutoPairing);
		AutoSaveToggleSwitch.Command = new RelayCommand(ToggleAutoSave);
	}

	private void ToggleRpc()
	{
		ToggleSetting("IsDiscordRpcEnabled");

		if (ApiVault.Get().GetAppConfig().IsDiscordRpcEnabled) DiscordRpcUpdater.Initialize();
		else DiscordRpcUpdater.Uninitialize();
	}

	private void ToggleWrapping()
	{
		ToggleSetting("IsWrappingEnabled");

		List<TextEditor> textEditors = ApiVault.Get().GetTabView().TabItems
			.OfType<TabViewItem>()
			.Select(x => x.Content as TextEditor)
			.Where(editor => editor != null)
			.ToList();

		textEditors.ForEach(e => e.WordWrap = ApiVault.Get().GetAppConfig().IsWrappingEnabled);
	}

	private void ToggleAutoIndent() => ToggleSetting("IsAutoIndentEnabled");
	private void ToggleAutoPairing() => ToggleSetting("IsAutoPairingEnabled");
	private void ToggleAutoSave() => ToggleSetting("IsAutoSaveEnabled");
	private void ToggleRealtimeAnalyzer() => ToggleSetting("IsRealtimeAnalyzerEnabled");

	private static void ToggleSetting(string propertyName)
	{
		var appConfig = ApiVault.Get().GetAppConfig();
		var property = appConfig.GetType().GetProperty(propertyName);

		if (property == null || property.PropertyType != typeof(bool)) return;
		var currentValue = (bool)property.GetValue(appConfig);
		property.SetValue(appConfig, !currentValue);
	}
}
