using System;

namespace SkEditor.ViewModels.Tabs.Capabilities;

public interface IFocusableTab
{
    event EventHandler? FocusRequested;
    void RequestFocus();
}