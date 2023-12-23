namespace SkEditor.API;
public interface IAddon
{
    public string Name { get; }
    public string Version { get; }

    public void OnEnable();
}