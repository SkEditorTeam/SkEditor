using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using SkEditor.API;

namespace SkEditor.Utilities;

/// <summary>
///     Represent a panel in the sidebar, with an icon and a user control as content.
/// </summary>
public abstract class SidebarPanel
{
    public abstract UserControl Content { get; }
    public abstract IconSource Icon { get; }
    public abstract IconSource IconActive { get; }
    public abstract bool IsDisabled { get; }

    public virtual int DesiredWidth => 250;

    public void OnOpen()
    {
    }

    public void OnClose()
    {
    }

    public string? GetId()
    {
        return Registries.SidebarPanels.GetValueKey(this)?.Key;
    }
}