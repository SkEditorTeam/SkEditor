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
        return Assembly.GetExecutingAssembly().GetName().Version;
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