namespace SkEditor.API;

/// <summary>
/// Represents a (remote) NuGet dependency. This
/// will download the desired package from the NuGet with
/// the provided package ID and version.
///
/// The provided package version can be null, which will
/// make SkEditor find the <b>latest</b> version of the package.
/// </summary>
public class NuGetDependency(string packageId, string nameSpace, string? version, bool isRequired = true) : IDependency
{

    public string PackageId { get; } = packageId;
    public string NameSpace { get; } = nameSpace;
    public string? Version { get; } = version;
    public bool IsRequired { get; } = isRequired;

}