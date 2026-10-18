using SkEditor.Utilities.Startup;
using BinarySettings = SkEditor.Utilities.Settings.Configs.BinarySettings;

namespace SkEditor.Services.Windowing;

public class SplashScreenService(BinarySettings binarySettings)
{
    private const int AutoDisableThreshold = 500;

    public bool ShouldShowSplash()
    {
        return binarySettings.SplashScreenMode switch
        {
            StartupMode.EnableSplashScreen => true,
            StartupMode.DisableSplashScreen => false,
            StartupMode.Auto => binarySettings.AverageStartupTime is 0 or > AutoDisableThreshold,
            _ => true
        };
    }

    public void UpdateStartupTime(int currentTime)
    {
        int averageStartupTime = binarySettings.AverageStartupTime;

        binarySettings.AverageStartupTime =
            averageStartupTime == 0 ? currentTime : ((averageStartupTime * 4) + currentTime) / 5;
        binarySettings.Save();
    }
}