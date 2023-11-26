using Avalonia;
using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using Serilog;
using SkEditor.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SkEditor.Desktop;

class Program
{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main(string[] args)
	{
		try
		{
			BuildAvaloniaApp()
				.StartWithClassicDesktopLifetime(args);
		}
		catch (Exception e)
		{
			Log.Fatal(e, "Application crashed!");

			List<TabViewItem> tabs = ApiVault.Get().GetTabView().TabItems
				.OfType<TabViewItem>()
				.Where(tab => tab.Content is TextEditor)
				.ToList();

			tabs.ForEach(tab =>
			{
				string path = tab.Tag.ToString();
				if (string.IsNullOrEmpty(path))
				{
					string tempPath = Path.Combine(Path.GetTempPath(), "SkEditor");
					Directory.CreateDirectory(tempPath);
					string header = tab.Header.ToString().TrimEnd('*');
					path = Path.Combine(tempPath, header);
				}
				TextEditor editor = tab.Content as TextEditor;
				string textToWrite = editor.Text;
				using StreamWriter writer = new(path, false);
				writer.Write(textToWrite);
			});

			ApiVault.Get().GetAppConfig().Save();

			Process.Start(Environment.ProcessPath, "--crash");
		}
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.LogToTrace()
			.WithInterFont();

}
