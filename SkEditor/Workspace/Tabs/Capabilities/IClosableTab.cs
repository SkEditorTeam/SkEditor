using System;
using System.Threading.Tasks;

namespace SkEditor.Workspace.Tabs.Capabilities;

public interface IClosableTab
{
    event Func<Task>? ClosingAsync;
}