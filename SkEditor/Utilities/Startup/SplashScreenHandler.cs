using SkEditor.API;
using SkEditor.Utilities.Settings;
using BinarySettings = SkEditor.Utilities.Settings.Configs.BinarySettings;

namespace SkEditor.Utilities.Startup;

public static class SplashScreenHandler
{
    private const int AutoDisableThreshold = 500;
    private static BinarySettings BinarySettings => SkEditorAPI.Configs.GetBinarySettings();
    
    public static bool ShouldShowSplash() => BinarySettings.SplashScreenMode switch
    {
        StartupMode.EnableSplashScreen => true,
        StartupMode.DisableSplashScreen => false,
        StartupMode.Auto => BinarySettings.AverageStartupTime is 0 or > AutoDisableThreshold,
        _ => true
    };

    public static void UpdateStartupTime(int currentTime)
    {
        int averageStartupTime = BinarySettings.AverageStartupTime;
        
        BinarySettings.AverageStartupTime = averageStartupTime == 0 ? currentTime : (averageStartupTime * 4 + currentTime) / 5;
        BinarySettings.Save();
    }
}