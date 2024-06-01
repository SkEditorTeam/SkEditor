using System;
using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Styling;
using SkEditor.Utilities;

namespace SkEditor.API;

public class Core : ICore
{
    private AppConfig? _appConfig;
    private string[] _startupArguments = null!;
    
    public AppConfig GetAppConfig()
    {
        return _appConfig ??= AppConfig.Load().Result;
    }

    public Version GetAppVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return new Version(version.Major, version.Minor, version.Build);
    }

    public string[] GetStartupArguments()
    {
        return _startupArguments ?? [];
    }
    
    public void SetStartupArguments(string[]? args)
    {
        _startupArguments = args ?? [];
    }

    public object? GetApplicationResource(string key)
    {
        Application.Current.TryGetResource(key, ThemeVariant.Dark, out var resource);
        return resource;
    }

    public void OpenLink(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    public bool IsDeveloperMode()
    {
        #if DEBUG
        return true;
        #endif
        
        return GetAppConfig().IsDevModeEnabled;
    }
}