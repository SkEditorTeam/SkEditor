namespace SkEditor.Utilities.InternalAPI;

public interface IAddonLoadingError
{
    bool IsCritical { get; }
    string Message { get; }
}