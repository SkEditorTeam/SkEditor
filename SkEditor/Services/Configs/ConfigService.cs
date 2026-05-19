using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Settings;

namespace SkEditor.Services;

public class ConfigService(IEnumerable<IConfig> configs)
    : IConfigService
{
    private readonly List<IConfig> _configs = configs.ToList();

    public string GetAppDataPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor", "v3.0");
    }

    public async Task LoadAllConfigsAsync() =>
        await Task.WhenAll(_configs.Select(c => c.Load()));

    public async Task SaveAllConfigsAsync() =>
        await Task.WhenAll(_configs.Select(c => c.Save()));

    public async Task SaveAllConfigsImmediateAsync()
    {
        await Task.WhenAll(_configs.Select(config => config.SaveImmediate()));
    }

    public void AddConfig<T>(T config) where T : IConfig
    {
        if (_configs.Any(c => c.GetType() == typeof(T)))
            throw new InvalidOperationException($"Config {typeof(T).Name} already exists!");

        _configs.Add(config);
    }

    public void RemoveConfig<T>() where T : IConfig
    {
        _configs.RemoveAll(c => c is T);
    }

    public T? GetConfig<T>() where T : IConfig
    {
        return _configs.OfType<T>().FirstOrDefault();
    }
}