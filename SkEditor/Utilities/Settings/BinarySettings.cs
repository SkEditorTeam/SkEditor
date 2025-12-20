using System;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Utilities.Settings;

/// <summary>
///    Manages application settings stored in a binary file next to the executable.
/// </summary>
public partial class BinarySettings : ObservableObject
{
    [ObservableProperty]
    private bool _isSplashScreenEnabled = true;

    private readonly string _binaryFilePath = Path.Combine(AppContext.BaseDirectory, "settings.bin");
    
    // Maps 
    private readonly Dictionary<int, (Func<bool> Getter, Action<bool> Setter)> _options;

    public BinarySettings()
    {
        _options = new Dictionary<int, (Func<bool> Getter, Action<bool> Setter)>
        {
            { 0, (() => _isSplashScreenEnabled, v => _isSplashScreenEnabled = v) },
        };
    }

    /// <summary>
    ///     Loads the settings if the file exists.
    /// </summary>
    public void Load()
    {
        if (!File.Exists(_binaryFilePath)) return;

        byte[] fileBytes = File.ReadAllBytes(_binaryFilePath);

        foreach ((int index, (_, Action<bool> setter)) in _options)
        {
            if (index < fileBytes.Length)
            {
                setter(fileBytes[index] != 0);
            }
        }
    }

    /// <summary>
    ///     Saves the settings to the file.
    /// </summary>
    public void Save()
    {
        byte[] bytes = new byte[_options.Count];

        foreach ((int index, (Func<bool> getter, _)) in _options)
        {
            bytes[index] = getter() ? (byte)1 : (byte)0;
        }

        File.WriteAllBytes(_binaryFilePath, bytes);
    }
}