using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkEditor.API;
using SkEditor.Utilities.Settings;
using AppSettings = SkEditor.Utilities.Settings.Configs.AppSettings;
using BinarySettings = SkEditor.Utilities.Settings.Configs.BinarySettings;
using SecretsConfig = SkEditor.Utilities.Settings.Configs.SecretsConfig;
using StateConfig = SkEditor.Utilities.Settings.Configs.StateConfig;

namespace SkEditor.Utilities.InternalAPI.Classes;

public class Configs : IConfigs
{
    private readonly List<IConfig> _configs = 
    [ 
        new AppSettings(), 
        new SecretsConfig(),
        new ExperimentsConfig(),
        new StateConfig()
    ];
    
    public string GetAppDataPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor");
    
    public async Task LoadAllConfigsAsync()
    {
        await Task.WhenAll(_configs.Select(config => config.Load()));
    }

    public async Task SaveAllConfigsAsync()
    {
        await Task.WhenAll(_configs.Select(config => config.Save()));
    }

    public AppSettings GetAppSettings() => GetConfig<AppSettings>();
    public SecretsConfig GetSecretsConfig() => GetConfig<SecretsConfig>();
    public ExperimentsConfig GetExperimentsConfig() => GetConfig<ExperimentsConfig>();
    public StateConfig GetStateConfig() => GetConfig<StateConfig>();
    public BinarySettings GetBinarySettings() => GetConfig<BinarySettings>();

    public T? GetConfig<T>() where T : IConfig
    {
        return (T?)_configs.FirstOrDefault(config => config is T);
    }

    public void AddConfig<T>(T config) where T : IConfig
    {
        if (_configs.Any(c => c.GetType() == typeof(T)))
        {
            throw new InvalidOperationException($"Config of type {typeof(T).Name} already exists.");
        }
    
        _configs.Add(config);
    }

    public void RemoveConfig<T>() where T : IConfig
    {
        var config = _configs.FirstOrDefault(c => c is T);
        if (config != null)
        {
            _configs.Remove(config);
        }
    }
}