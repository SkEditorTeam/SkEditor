using System;
using Microsoft.Extensions.DependencyInjection;
using BinarySettings = SkEditor.Utilities.Settings.Configs.BinarySettings;

namespace SkEditor.Utilities.Startup;

public static class SplashScreenHandler
{
    private const int AutoDisableThreshold = 500;

    [Obsolete("use proper config injection when rewriting to DI")]
    private static BinarySettings BinarySettings => App.Services.GetRequiredService<BinarySettings>();

    public static bool ShouldShowSplash()
    {
        return BinarySettings.SplashScreenMode switch
        {
            StartupMode.EnableSplashScreen => true,
            StartupMode.DisableSplashScreen => false,
            StartupMode.Auto => BinarySettings.AverageStartupTime is 0 or > AutoDisableThreshold,
            _ => true
        };
    }

    public static void UpdateStartupTime(int currentTime)
    {
        int averageStartupTime = BinarySettings.AverageStartupTime;

        BinarySettings.AverageStartupTime =
            averageStartupTime == 0 ? currentTime : ((averageStartupTime * 4) + currentTime) / 5;
        BinarySettings.Save();
    }
}