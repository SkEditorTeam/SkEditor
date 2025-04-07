using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using SkEditor.API;

namespace SkEditor.Utilities;

/// <summary>
/// Represent a panel in the sidebar, with an icon and a user control as content.
/// </summary>
public abstract class SidebarPanel
{

    public abstract UserControl Content { get; }
    public abstract IconSource Icon { get; }
    public abstract IconSource IconActive { get; }
    public abstract bool IsDisabled { get; }

    public virtual int DesiredWidth { get; } = 250;

    public virtual void OnOpen()
    {

    }

    public virtual void OnClose()
    {

    }

    public string GetId() => Registries.SidebarPanels.GetValueKey(this).Key;

}