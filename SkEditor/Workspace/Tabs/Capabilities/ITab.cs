using SkEditor.Core.Localization;

namespace SkEditor.Workspace.Tabs.Capabilities;

public interface ITab
{
    TranslatableString Header { get; }

    bool IsCacheable { get; }
    bool IsLoading { get; set; }
}