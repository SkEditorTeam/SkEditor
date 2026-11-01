using Semver;

namespace SkEditor.Core.Common;

/// <summary>
/// Contains app information like version, installation type, etc.
/// </summary>
public interface IAppInfo
{
    /// <summary>
    /// The version of the application
    /// </summary>
    SemVersion Version { get; }
    
    /// <summary>
    /// Whether it was detected that the application was installed on Windows (via an installer) or not (portable version/other OS)
    /// </summary>
    bool IsInstalled { get; }
}

public sealed class AppInfo(SemVersion version, bool isInstalled = false) : IAppInfo
{
    public SemVersion Version { get; } = version;
    public bool IsInstalled { get; } = isInstalled;
}