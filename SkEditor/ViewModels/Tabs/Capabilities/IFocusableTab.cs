using System;

namespace SkEditor.ViewModels.Tabs.Capabilities;

public interface IFocusableTab
{
    event Action? FocusRequested;
    void RequestFocus();
}