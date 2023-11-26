using Avalonia.Controls;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Syntax;
using SkEditor.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkEditor.Views.Settings;
public partial class PersonalizationPage : UserControl
{
	public PersonalizationPage()
	{
		InitializeComponent();

		DataContext = new SettingsViewModel();

		AssignCommands();
		LoadSyntaxes();
	}

	private void LoadSyntaxes()
	{
		foreach (string syntax in SyntaxLoader.Syntaxes)
		{
			ComboBoxItem item = new()
			{
				Content = Path.GetFileNameWithoutExtension(syntax),
				Tag = syntax
			};

			SyntaxComboBox.Items.Add(item);
		}

		SyntaxComboBox.SelectedItem = SyntaxComboBox.Items.FirstOrDefault(x =>
										((ComboBoxItem)x).Tag.Equals(ApiVault.Get().GetAppConfig().CurrentSyntax));

		SyntaxComboBox.SelectionChanged += (s, e) =>
		{
			ComboBoxItem item = (ComboBoxItem)SyntaxComboBox.SelectedItem;
			ApiVault.Get().GetAppConfig().CurrentSyntax = item.Tag.ToString();
			SyntaxLoader.UpdateSyntax(SyntaxLoader.SyntaxFilePath);
		};
	}

	private void AssignCommands()
	{
		ThemePageButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(ThemePage)));
		Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(HomePage)));

		FontButton.Command = new RelayCommand(SelectFont);
	}

	private async void SelectFont()
	{
		FontSelectionWindow window = new();
		string result = await window.ShowDialog<string>(ApiVault.Get().GetMainWindow());
		if (result is null)
			return;

		ApiVault.Get().GetAppConfig().Font = result;
		CurrentFont.Description = Translation.Get("SettingsPersonalizationFontDescription").Replace("{0}", result);

		List<TextEditor> textEditors = ApiVault.Get().GetTabView().TabItems.Cast<TabViewItem>()
			.Where(i => i.Content is TextEditor)
			.Select(i => i.Content as TextEditor).ToList();

		textEditors.ForEach(i => i.FontFamily = new(result));
	}
}
