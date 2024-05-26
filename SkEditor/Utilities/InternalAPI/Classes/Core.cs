using System;
using System.Reflection;
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
}