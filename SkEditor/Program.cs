using Avalonia;
using Avalonia.Data;
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

            // Check for the --test argument
            if (args.Length > 0 && args[0] == "--test")
            {
                try
                {
                    // Here you can add any initialization code that you want to test
                    Console.WriteLine("Test passed");
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Test failed: {e.Message}");
                    Environment.Exit(1);
                }
                return;
            }

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

                var fullException = e.ToString();
                var encodedMessage = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fullException));
                Process.Start(Environment.ProcessPath, "--crash " + encodedMessage);
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
