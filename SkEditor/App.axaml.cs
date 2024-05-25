using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Serilog;
using SkEditor.Utilities;
using SkEditor.Views;
using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using SkEditor.API;

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
            .WriteTo.Sink(new LogsHandler())
            .CreateLogger();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            bool isFirstInstance;
            try
            {
                isFirstInstance = mutex.WaitOne(TimeSpan.Zero, true);
            }
            catch (AbandonedMutexException ex)
            {
                ex.Mutex?.Close();
                isFirstInstance = true;
            }

            if (isFirstInstance)
            {
                try
                {
                    (SkEditorAPI.Core as Core).SetStartupArguments(desktop.Args ?? []);
                    new SkEditor();
                    
                    MainWindow mainWindow = new();
                    desktop.MainWindow = mainWindow;

                    NamedPipeServer.Start();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error creating SkEditor");
                    desktop.Shutdown();
                }

                desktop.Exit += (sender, e) =>
                {
                    mutex.ReleaseMutex();
                    mutex.Dispose();
                };
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