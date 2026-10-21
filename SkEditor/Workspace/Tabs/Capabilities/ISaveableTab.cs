namespace SkEditor.Workspace.Tabs.Capabilities;

public interface ISaveableTab
{
    bool IsSaved { get; set; }
    void MarkOriginal();
}