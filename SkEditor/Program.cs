using Avalonia;
using Serilog;
using SkEditor.API;
using System;
using System.Diagnostics;

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
            Stopwatch stopwatch = Stopwatch.StartNew();
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
            stopwatch.Stop();
            Log.Information("Application started in {0}ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Application crashed!");

            ApiVault.Get().SaveData();

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
