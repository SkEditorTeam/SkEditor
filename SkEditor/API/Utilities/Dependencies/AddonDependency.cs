namespace SkEditor.API;

/// <summary>
/// Represents a dependency to another addon.
/// </summary>
public class AddonDependency(string addonIdentifier, bool isRequired = true) : IDependency
{
    public string AddonIdentifier { get; } = addonIdentifier;
    public bool IsRequired { get; } = isRequired;
}