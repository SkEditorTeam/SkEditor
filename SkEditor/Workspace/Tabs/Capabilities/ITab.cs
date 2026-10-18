using System;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Core.Localization;

namespace SkEditor.Workspace.Tabs.Capabilities;

public interface ITab
{
    TranslatableString Header { get; }
    IRelayCommand CloseCommand { get; }

    bool IsCacheable { get; }
    bool IsLoading { get; }
    event EventHandler? CloseRequested;
}