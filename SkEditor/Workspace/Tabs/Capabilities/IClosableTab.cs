using System;
using System.Threading.Tasks;

namespace SkEditor.Workspace.Tabs.Capabilities;

public interface IClosableTab
{
    event Func<Task>? ClosingAsync
    {
        add => _ = value;
        remove => _ = value;
    }
    
    Task<bool> CanCloseAsync() => Task.FromResult(true);
    void OnClosed() { }
}