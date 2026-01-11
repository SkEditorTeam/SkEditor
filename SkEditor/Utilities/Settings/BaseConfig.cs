using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.API;

namespace SkEditor.Utilities.Settings;

public abstract class BaseConfig<T> : ObservableObject, IConfig where T : BaseConfig<T>, IConfig, new()
{
    protected abstract JsonTypeInfo<T> TypeInfo { get; }
    public abstract string FileName { get; }
    
    // ReSharper disable once ReplaceWithFieldKeyword
    private readonly string _appDataFolderPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor");
    
    private string ConfigPath => Path.Combine(_appDataFolderPath, FileName);
    
    public async Task Load()
    {
        if (!File.Exists(ConfigPath)) return;

        try 
        {
            string json = await File.ReadAllTextAsync(ConfigPath);
            
            var loadedData = JsonSerializer.Deserialize(json, TypeInfo);

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
        var json = JsonSerializer.Serialize((T)this, TypeInfo);
        
        var directory = Path.GetDirectoryName(ConfigPath);
        if (directory != null) Directory.CreateDirectory(directory);
        
        await File.WriteAllTextAsync(ConfigPath, json);
    }
    
    protected abstract void CopyFrom(T other);
}