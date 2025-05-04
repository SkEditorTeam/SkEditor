using System;
using Avalonia;
using Avalonia.Data;

namespace SkEditor;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        GC.KeepAlive(typeof(RelativeSource));
        CheckTest(args);

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    private static void CheckTest(string[] args)
    {
        if (args.Length > 0 && args[0] == "--test")
        {
            try
            {
                Console.WriteLine("Test passed");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Test failed: {e.Message}");
                Environment.Exit(1);
            }
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .WithInterFont();
    }
}