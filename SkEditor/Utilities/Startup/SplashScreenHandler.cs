namespace SkEditor.Utilities.Startup;

public static class SplashScreenHandler
{
    private const int AutoDisableThreshold = 500;
    
    public static bool ShouldShowSplash() => App.BinarySettings?.SplashScreenMode switch
    {
        StartupMode.EnableSplashScreen => true,
        StartupMode.DisableSplashScreen => false,
        StartupMode.Auto => App.BinarySettings.AverageStartupTime is 0 or > AutoDisableThreshold,
        _ => true
    };

    public static void UpdateStartupTime(int currentTime)
    {
        int averageStartupTime = App.BinarySettings?.AverageStartupTime ?? 0;
        
        App.BinarySettings?.AverageStartupTime = averageStartupTime == 0 ? currentTime : (averageStartupTime * 4 + currentTime) / 5;
        App.BinarySettings?.Save();
    }
}