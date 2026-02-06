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

public partial class Configs : IConfigs
{
    private readonly List<IConfig> _configs =
    [
        new AppSettings(),
        new SecretsConfig(),
        new ExperimentsConfig(),
        new StateConfig()
    ];

    public string GetAppDataPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor");
    }

    public async Task LoadAllConfigsAsync()
    {
        await Task.WhenAll(_configs.Select(config => config.Load()));
    }

    public async Task SaveAllConfigsAsync()
    {
        await Task.WhenAll(_configs.Select(config => config.Save()));
    }
    
    public async Task SaveAllConfigsImmediateAsync()
    {
        await Task.WhenAll(_configs.Select(config => config.SaveImmediate()));
    }

    public AppSettings GetAppSettings()
    {
        return GetConfig<AppSettings>() ??
               throw new InvalidOperationException("AppSettings config not found.");
    }

    public SecretsConfig GetSecretsConfig()
    {
        return GetConfig<SecretsConfig>() ??
               throw new InvalidOperationException("SecretsConfig config not found.");
    }

    public ExperimentsConfig GetExperimentsConfig()
    {
        return GetConfig<ExperimentsConfig>() ??
               throw new InvalidOperationException("ExperimentsConfig config not found.");
    }

    public StateConfig GetStateConfig()
    {
        return GetConfig<StateConfig>() ??
               throw new InvalidOperationException("StateConfig config not found.");
    }

    public BinarySettings GetBinarySettings()
    {
        return GetConfig<BinarySettings>() ??
               throw new InvalidOperationException(
                   "BinarySettings config not found.");
    }

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
        IConfig? config = _configs.FirstOrDefault(c => c is T);
        if (config != null)
        {
            _configs.Remove(config);
        }
    }
}