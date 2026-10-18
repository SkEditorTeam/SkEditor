using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;
using SkEditor.Configuration.Abstractions;
using SkEditor.Core.Common;
using SkEditor.Windowing.Models;

namespace SkEditor.Configuration.Configs;

/// <summary>
///     Manages application settings stored in a binary file next to the executable.
/// </summary>
[SettingsConfig(false)]
public partial class BinarySettings : ObservableObject, IConfig
{
    private const string ConfigFileName = "settings.bin";

    private readonly string _binaryFilePath = Path.Combine(AppPaths.LocalData, ConfigFileName);

    [ObservableProperty]
    public partial int AverageStartupTime { get; set; }

    public string FileName => ConfigFileName;

    [ObservableProperty]
    public partial StartupMode SplashScreenMode { get; set; } = StartupMode.Auto;

    public int SplashScreenModeIndex
    {
        get => (int)SplashScreenMode;
        set => SplashScreenMode = (StartupMode)value;
    }

    private List<IBinarySettingHandler> Registry => field ??=
    [
        new BinarySettingHandler<byte>(() => (byte)SplashScreenMode, v => SplashScreenMode = (StartupMode)v,
            r => r.ReadByte(), (w, v) => w.Write(v)),

        new BinarySettingHandler<int>(() => AverageStartupTime, v => AverageStartupTime = v, r => r.ReadInt32(),
            (w, v) => w.Write(v))
    ];

    /// <summary>
    ///     Loads the settings if the file exists.
    /// </summary>
    public Task Load()
    {
        string legacyPath = Path.Combine(AppPaths.RoamingData, ConfigFileName);

        if (!File.Exists(_binaryFilePath) && File.Exists(legacyPath))
        {
            try
            {
                string? dir = Path.GetDirectoryName(_binaryFilePath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.Move(legacyPath, _binaryFilePath, true);
            }
            catch (Exception ex)
            {
                Log.Logger.Warning(ex, "Failed to migrate settings.bin from Roaming");
            }
        }

        if (!File.Exists(_binaryFilePath))
        {
            return Task.CompletedTask;
        }

        try
        {
            using BinaryReader reader = new(File.OpenRead(_binaryFilePath));
            foreach (IBinarySettingHandler handler in Registry.Where(_ =>
                         reader.BaseStream.Position < reader.BaseStream.Length))
            {
                handler.Load(reader);
            }
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Error loading binary settings");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Saves the settings to the file.
    /// </summary>
    public Task Save()
    {
        try
        {
            string? directory = Path.GetDirectoryName(_binaryFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using BinaryWriter writer = new(File.Create(_binaryFilePath));
            foreach (IBinarySettingHandler handler in Registry)
            {
                handler.Save(writer);
            }
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Error saving binary settings");
        }

        return Task.CompletedTask;
    }

    public Task SaveImmediate()
    {
        return Save();
    }
}