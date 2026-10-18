using System;

namespace SkEditor.Workspace.Tabs.Capabilities;

public interface IFocusableTab
{
    void RequestFocus();
    event EventHandler? FocusRequested;
}