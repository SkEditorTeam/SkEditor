using Avalonia;
using Avalonia.Data;
using Avalonia.Threading;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities.InternalAPI;
using System;
using System.Diagnostics;

namespace SkEditor.Desktop
{
    class Program
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
                return;
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .WithInterFont();
    }
}
