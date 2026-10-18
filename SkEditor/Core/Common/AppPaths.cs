using System;
using System.IO;

namespace SkEditor.Core.Common;

public static class AppPaths
{
    private const string AppFolderName = "SkEditor";
    private const string VersionFolderName = "v3.0";

    public static string RoamingData { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        AppFolderName, VersionFolderName);

    public static string LocalData { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppFolderName, VersionFolderName);

    public static string Logs { get; } = Path.Combine(LocalData, "Logs");
}