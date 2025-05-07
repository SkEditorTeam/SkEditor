using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;
using Serilog;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;

namespace SkEditor.API;

public class Core : ICore
{
    private AppConfig? _appConfig;
    private string[] _startupArguments = null!;

    public AppConfig GetAppConfig()
    {
        return _appConfig ??= AppConfig.Load();
    }

    public Version GetAppVersion()
    {
        Version? version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version == null)
        {
            throw new InvalidOperationException("Version is null");
        }
        return new Version(version.Major, version.Minor, version.Build);
    }

    public string GetInformationalVersion()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string? informationVersion =
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        
        if (informationVersion == null)
        {
            throw new InvalidOperationException("Informational version is null");
        }

        return informationVersion;
    }

    public string[] GetStartupArguments()
    {
        return _startupArguments;
    }

    public void SetStartupArguments(string[]? args)
    {
        _startupArguments = args ?? [];
    }

    public object? GetApplicationResource(string key)
    {
        object? resource = null;
        Application.Current?.TryGetResource(key, ThemeVariant.Dark, out resource);
        return resource;
    }

    public void OpenLink(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    public void OpenFolder(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
            Verb = "open"
        });
    }

    public bool IsDeveloperMode()
    {
#pragma warning disable 162
#if DEBUG
        return true;
#endif
        // ReSharper disable once HeuristicUnreachableCode
        return GetAppConfig().IsDevModeEnabled;
#pragma warning restore 162
    }

    public async Task SaveData()
    {
        foreach (OpenedFile file in SkEditorAPI.Files.GetOpenedEditors())
        {
            string? path = file.Path;
            if (string.IsNullOrEmpty(path))
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "SkEditor");
                Directory.CreateDirectory(tempPath);
                path = Path.Combine(tempPath, file.Header);
            }

            string? textToWrite = file.Editor?.Text;
            if (string.IsNullOrEmpty(textToWrite))
            {
                continue;
            }

            try
            {
                await File.WriteAllTextAsync(path, textToWrite);
                await using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to save file: {path}");
            }
        }

        GetAppConfig().Save();
    }
}