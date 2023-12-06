using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;

namespace SkEditor;

public partial class App : Application
{
	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	static Mutex mutex = new(true, "{217619cc-ff9d-438b-8a0a-348df94de61b}");

	public override async void OnFrameworkInitializationCompleted()
	{
		base.OnFrameworkInitializationCompleted();

		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Debug()
			.WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"SkEditor/Logs/log-.txt"), rollingInterval: RollingInterval.Minute)
			.CreateLogger();

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			if (mutex.WaitOne(TimeSpan.Zero, true))
			{
				try
				{
					SkEditor SkEditor = new(desktop.Args);
					MainWindow mainWindow = new();
					desktop.MainWindow = mainWindow;
					SkEditor.mainWindow = mainWindow;

					NamedPipeServer.Start();
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error creating SkEditor");
					desktop.Shutdown();
				}

				mutex.ReleaseMutex();
			}
			else
			{
				var args = Environment.GetCommandLineArgs();
				try
				{
					using NamedPipeClientStream namedPipeClientStream = new("SkEditor");
					await namedPipeClientStream.ConnectAsync();
					if (args != null && args.Length > 1)
					{
						byte[] buffer = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, args.Skip(1)));
						namedPipeClientStream.Write(buffer, 0, buffer.Length);
					}
					else
					{
						namedPipeClientStream.WriteByte(0);
					}
				}
				catch (Exception ex)
				{
					Log.Debug(ex, "Error connecting to named pipe");
				}

				desktop.Shutdown();
			}
		}
	}

}