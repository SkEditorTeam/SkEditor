using Avalonia;
using Serilog;
using SkEditor.API;
using System;
using System.Diagnostics;
using Avalonia.Data;
using SkEditor.Utilities.InternalAPI;

namespace SkEditor.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    public static void Main(string[] args)
    {
        GC.KeepAlive(typeof(RelativeSource));
        
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            string message = "Application crashed!";
            string? source = e.Source;
            if (AddonLoader.DllNames.Contains(source + ".dll"))
            {
                message += $" It's fault of {source} addon.";
            }
            Log.Fatal(e, message);
            Console.Error.WriteLine(e);
            Console.Error.WriteLine(message);

            SkEditorAPI.Core.SaveData();
            AddonLoader.SaveMeta();

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
