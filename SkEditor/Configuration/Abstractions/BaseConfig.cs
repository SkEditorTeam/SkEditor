using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Core.Common;
using SkEditor.Core.Logging;

namespace SkEditor.Configuration.Abstractions;

public abstract class BaseConfig<T> : ObservableObject, IConfig where T : BaseConfig<T>, IConfig, new()
{
    private const int SaveDebounceMs = 300;

    protected virtual Environment.SpecialFolder TargetFolder => Environment.SpecialFolder.ApplicationData;

    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private CancellationTokenSource? _saveCts;

    [JsonIgnore]
    public string FileName => GetFileName();

    /// <summary>
    ///     Type information for JSON serialization and deserialization.
    /// </summary>
    protected abstract JsonTypeInfo<T> TypeInfo { get; }

    private string ConfigPath => Path.Combine(AppPaths.RoamingData, FileName);

    public async Task Load()
    {
        TryMigrateFromRoaming();
        
        if (!File.Exists(ConfigPath))
        {
            T defaultData = new();
            CopyFrom(defaultData);
            await SaveInternal();
            return;
        }

        try
        {
            string json = await File.ReadAllTextAsync(ConfigPath);

            T? loadedData = JsonSerializer.Deserialize(json, TypeInfo);

            if (loadedData != null)
            {
                CopyFrom(loadedData);
            }
        }
        catch (Exception ex)
        {
            App.Services.GetService<ILogService>()?.Error($"Failed to load config file '{ConfigPath}': {ex}");
        }
    }

    // TEMP: Remove migration from Roaming when leaving Alpha
    private void TryMigrateFromRoaming()
    {
        if (TargetFolder != Environment.SpecialFolder.LocalApplicationData)
        {
            return;
        }

        if (File.Exists(ConfigPath))
        {
            return;
        }

        string legacyPath = Path.Combine(AppPaths.RoamingData, FileName);

        if (!File.Exists(legacyPath))
        {
            return;
        }

        try
        {
            string? targetDir = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            File.Move(legacyPath, ConfigPath, true);
            App.Services.GetService<ILogService>()
                ?.Info($"Migrated '{FileName}' from Roaming to LocalApplicationData.");
        }
        catch (Exception ex)
        {
            App.Services.GetService<ILogService>()
                ?.Warning($"Failed to migrate legacy '{FileName}' from Roaming: {ex.Message}");
        }
    }
    
    public async Task Save()
    {
        if (_saveCts != null)
        {
            await _saveCts.CancelAsync();
        }

        _saveCts = new CancellationTokenSource();
        CancellationToken token = _saveCts.Token;

        try
        {
            await Task.Delay(SaveDebounceMs, token);
            await SaveInternal();
        }
        catch (TaskCanceledException)
        {
            // ignore
        }
    }

    public async Task SaveImmediate()
    {
        if (_saveCts != null)
        {
            await _saveCts.CancelAsync();
        }

        await SaveInternal();
    }

    protected abstract void CopyFrom(T other);
    protected abstract string GetFileName();

    private async Task SaveInternal()
    {
        await _saveLock.WaitAsync();
        try
        {
            string json = JsonSerializer.Serialize((T)this, TypeInfo);
            string? directory = Path.GetDirectoryName(ConfigPath);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(ConfigPath, json);
        }
        finally
        {
            _saveLock.Release();
        }
    }
}