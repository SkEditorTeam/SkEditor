namespace SkEditor.API;

/// <summary>
/// Represents a dependency to another addon.
/// </summary>
public class AddonDependency : IDependency
{
    public AddonDependency(string addonIdentifier, bool isRequired = true)
    {
        AddonIdentifier = addonIdentifier;
        IsRequired = isRequired;
    }
    
    public string AddonIdentifier { get; }
    public bool IsRequired { get; }
}