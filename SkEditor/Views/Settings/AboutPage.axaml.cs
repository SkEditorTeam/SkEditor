using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.ViewModels;

namespace SkEditor.Views.Settings;
public partial class AboutPage : UserControl
{
	public AboutPage()
	{
		InitializeComponent();

		AssignCommands();

		DataContext = new SettingsViewModel();
	}

	private void AssignCommands()
	{
		Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(HomePage)));

		GitHubItem.Command = new RelayCommand(() => ApiVault.Get().OpenUrl("https://github.com/NotroDev/SkEditor"));
		DiscordItem.Command = new RelayCommand(() => ApiVault.Get().OpenUrl("https://discord.gg/kJUKX3ePj6"));
	}
}
