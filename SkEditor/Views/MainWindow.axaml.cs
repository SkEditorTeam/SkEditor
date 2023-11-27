using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Controls.Primitives;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Controls;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Styling;
using SkEditor.Utilities.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SkEditor.Views;

public partial class MainWindow : AppWindow
{
	public BottomBarControl GetBottomBar() => this.FindControl<BottomBarControl>("BottomBar");

	public MainWindow()
	{
		InitializeComponent();

		WindowStyler.Style(this);
		ThemeEditor.LoadThemes();
		AddEvents();

		Translation.LoadDefaultLanguage();
		Translation.ChangeLanguage(ApiVault.Get().GetAppConfig().Language);
	}

	private void AddEvents()
	{
		TabControl.AddTabButtonCommand = new RelayCommand(FileHandler.NewFile);
		TabControl.TabCloseRequested += (sender, e) => FileHandler.CloseFile(e);
		TemplateApplied += OnWindowLoaded;
		Closing += OnClosing;
		KeyDown += (sender, e) =>
		{
			if (e.KeyModifiers == KeyModifiers.Control && e.Key >= Key.D1 && e.Key <= Key.D9)
			{
				FileHandler.SwitchTab((int)e.Key - 35);
			}
		};

		DragDrop.SetAllowDrop(this, true);
		DragDrop.DropEvent.AddClassHandler(FileHandler.FileDropAction);
	}

	private async void OnClosing(object sender, WindowClosingEventArgs e)
	{
		ThemeEditor.SaveAllThemes();
		ApiVault.Get().GetAppConfig().Save();

		List<TabViewItem> unsavedFiles = ApiVault.Get().GetTabView().TabItems.Cast<TabViewItem>().Where(item => item.Header.ToString().EndsWith('*')).ToList();
		if (unsavedFiles.Count != 0)
		{
			e.Cancel = true;
			ContentDialogResult result = await ApiVault.Get().ShowMessageWithIcon(Translation.Get("Attention"), Translation.Get("ClosingProgramWithUnsavedFiles"), new SymbolIconSource() { Symbol = Symbol.ImportantFilled });
			if (result == ContentDialogResult.Primary)
			{
				unsavedFiles.ForEach(item => item.Header = item.Header.ToString().TrimEnd('*'));
				Close();
			}
			return;
		}
	}

	private async void OnWindowLoaded(object sender, RoutedEventArgs e)
	{
		AddonLoader.Load();

		ThemeEditor.SetTheme(ThemeEditor.CurrentTheme);

		string[] startupFiles = ApiVault.Get().GetStartupFiles();
		if (startupFiles.Length == 0) FileHandler.NewFile();
		startupFiles.ToList().ForEach(FileHandler.OpenFile);

		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			SyntaxLoader.LoadSyntaxes();
			DiscordRpcUpdater.Initialize();

			CrashChecker.CheckForCrash();
		});

		Tutorial.ShowTutorial();
		BottomBar.UpdatePosition();
	}
}