using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.API;

namespace SkEditor.Utilities.Settings;

public abstract class BaseConfig<T> : ObservableObject, IConfig where T : BaseConfig<T>, IConfig, new()
{
    private const int SaveDebounceMs = 300;

    // ReSharper disable once ReplaceWithFieldKeyword
    private readonly string _appDataFolderPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor");

    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private CancellationTokenSource? _saveCts;

    protected abstract JsonTypeInfo<T> TypeInfo { get; }

    private string ConfigPath => Path.Combine(_appDataFolderPath, FileName);
    public abstract string FileName { get; }

    public async Task Load()
    {
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
            SkEditorAPI.Logs.Error($"Failed to load config file '{ConfigPath}': {ex}");
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

    protected abstract void CopyFrom(T other);
}