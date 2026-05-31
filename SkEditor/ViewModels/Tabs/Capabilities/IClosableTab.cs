using System;
using System.Threading.Tasks;

namespace SkEditor.ViewModels.Tabs.Capabilities;

public interface IClosableTab
{
    event Func<Task>? ClosingAsync;
}