using System.Threading.Tasks;

namespace SkEditor.Workspace.Tabs.Capabilities;

public interface ISaveableTab
{
    Task<bool> SaveAsync();
    Task<bool> SaveAsAsync();
    bool IsSaved { get; set; }
}