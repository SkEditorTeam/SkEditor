using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Avalonia;
using Avalonia.Data;
using SkEditor.Core.Common;

namespace SkEditor;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        GC.KeepAlive(typeof(RelativeSource));

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        bool hardwareAcceleration = CheckHardwareAcceleration();
        
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions
            {
                RenderingMode = hardwareAcceleration
                    ? [Win32RenderingMode.AngleEgl, Win32RenderingMode.Software]
                    : [Win32RenderingMode.Software]
            })
            .With(new X11PlatformOptions
            {
                RenderingMode = hardwareAcceleration
                    ? [X11RenderingMode.Egl, X11RenderingMode.Glx, X11RenderingMode.Software]
                    : [X11RenderingMode.Software]
            })
            .With(new AvaloniaNativePlatformOptions
            {
                RenderingMode = hardwareAcceleration
                    ?
                    [
                        AvaloniaNativeRenderingMode.Metal, AvaloniaNativeRenderingMode.OpenGl,
                        AvaloniaNativeRenderingMode.Software
                    ]
                    : [AvaloniaNativeRenderingMode.Software]
            })
            .UseWaylandWithFallback()
            .LogToTrace()
            .UseR3();
    }

    private static bool CheckHardwareAcceleration()
    {
        try
        {
            string filePath = Path.Combine(AppPaths.RoamingData, "options.json");

            if (!File.Exists(filePath))
            {
                return true;
            }

            byte[] bytes = File.ReadAllBytes(filePath);
            Utf8JsonReader reader = new(bytes);

            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                if (reader.ValueTextEquals("useHardwareAcceleration"u8))
                {
                    reader.Read();
                    return reader.GetBoolean();
                }

                reader.Read();
                reader.Skip();
            }
        }
        catch
        {
            return true;
        }

        return true;
    }
}