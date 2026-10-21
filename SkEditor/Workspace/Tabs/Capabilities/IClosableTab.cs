using System;
using System.Threading.Tasks;

namespace SkEditor.Workspace.Tabs.Capabilities;

public interface IClosableTab
{
    Task<bool> CanCloseAsync();
    void OnClosed();
}