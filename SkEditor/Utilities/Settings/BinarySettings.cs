using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Startup;

namespace SkEditor.Utilities.Settings;

public partial class BinarySettings : ObservableObject
{
    public int SplashScreenModeIndex
    {
        get => (int)SplashScreenMode;
        set => SplashScreenMode = (StartupMode)value;
    }
    
    [ObservableProperty] private StartupMode _splashScreenMode = StartupMode.Auto;
    
    [ObservableProperty] private int _averageStartupTime;

    private readonly string _binaryFilePath = Path.Combine(AppContext.BaseDirectory, "settings.bin");
    private readonly List<IBinarySettingHandler> _registry;

    public BinarySettings()
    {
        _registry =
        [
            new BinarySettingHandler<byte>(() => (byte)SplashScreenMode, v => SplashScreenMode = (StartupMode)v,
                r => r.ReadByte(), (w, v) => w.Write(v)),

            new BinarySettingHandler<int>(() => AverageStartupTime, v => AverageStartupTime = v, r => r.ReadInt32(),
                (w, v) => w.Write(v))
        ];
    }

    public void Load()
    {
        if (!File.Exists(_binaryFilePath)) return;
        try
        {
            using BinaryReader reader = new(File.OpenRead(_binaryFilePath));
            foreach (IBinarySettingHandler handler in _registry.Where(_ => reader.BaseStream.Position < reader.BaseStream.Length))
            {
                handler.Load(reader);
            }
        }
        catch
        {
            // ignored
        }
    }

    public void Save()
    {
        try
        {
            using BinaryWriter writer = new(File.OpenWrite(_binaryFilePath));
            foreach (var handler in _registry)
            {
                handler.Save(writer);
            }
        }
        catch
        {
            // ignored
        }
    }
}