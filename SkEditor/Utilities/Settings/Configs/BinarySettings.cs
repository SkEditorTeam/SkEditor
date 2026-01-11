using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Startup;

namespace SkEditor.Utilities.Settings.Configs;

/// <summary>
///     Manages application settings stored in a binary file next to the executable.
/// </summary>
public partial class BinarySettings : ObservableObject, IConfig
{
    private const string ConfigFileName = "settings.bin";

    private readonly string _binaryFilePath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
    private readonly List<IBinarySettingHandler> _registry;
    [ObservableProperty] private int _averageStartupTime;

    [ObservableProperty] private bool _isFirstRun = true;

    [ObservableProperty] private StartupMode _splashScreenMode = StartupMode.Auto;

    public BinarySettings()
    {
        _registry =
        [
            new BinarySettingHandler<byte>(() => (byte)SplashScreenMode, v => SplashScreenMode = (StartupMode)v,
                r => r.ReadByte(), (w, v) => w.Write(v)),

            new BinarySettingHandler<int>(() => AverageStartupTime, v => AverageStartupTime = v, r => r.ReadInt32(),
                (w, v) => w.Write(v)),

            new BinarySettingHandler<bool>(() => IsFirstRun, v => IsFirstRun = v, r => r.ReadBoolean(),
                (w, v) => w.Write(v))
        ];
    }

    public int SplashScreenModeIndex
    {
        get => (int)SplashScreenMode;
        set => SplashScreenMode = (StartupMode)value;
    }

    public string FileName => ConfigFileName;

    /// <summary>
    ///     Loads the settings if the file exists.
    /// </summary>
    public Task Load()
    {
        if (!File.Exists(_binaryFilePath))
        {
            return Task.CompletedTask;
        }

        try
        {
            using BinaryReader reader = new(File.OpenRead(_binaryFilePath));
            foreach (IBinarySettingHandler handler in _registry.Where(_ =>
                         reader.BaseStream.Position < reader.BaseStream.Length))
            {
                handler.Load(reader);
            }
        }
        catch
        {
            // ignored
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
            using BinaryWriter writer = new(File.OpenWrite(_binaryFilePath));
            foreach (IBinarySettingHandler handler in _registry)
            {
                handler.Save(writer);
            }
        }
        catch
        {
            // ignored
        }

        return Task.CompletedTask;
    }
}