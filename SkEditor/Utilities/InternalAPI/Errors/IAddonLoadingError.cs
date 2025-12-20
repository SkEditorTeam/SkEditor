namespace SkEditor.Utilities.InternalAPI.Errors;

public interface IAddonLoadingError
{
    bool IsCritical { get; }
    string Message { get; }
}