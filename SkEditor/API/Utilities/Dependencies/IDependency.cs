namespace SkEditor.API;

/// <summary>
/// Represent a dependency of an addon. This can either be
/// a NuGet package reference, or a reference to another addon.
///
/// The dependency can either be required or optional.
/// <b>Be sure to check if the dependency is available before using it!</b>
/// </summary>
public interface IDependency
{

    /// <summary>
    /// Check if this dependency is required by the desired addon.
    /// If it is missing, the addon will not be enabled.
    /// </summary>
    public bool IsRequired { get; }

}