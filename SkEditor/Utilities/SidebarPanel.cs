using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace SkEditor.Utilities;

/// <summary>
/// Represent a panel in the sidebar, with an icon and a user control as content.
/// </summary>
public abstract class SidebarPanel
{
    
    public abstract UserControl Content { get; }
    public abstract IconSource Icon { get; }
    public abstract bool IsDisabled { get; }
    
    public virtual void OnOpen()
    {
        
    }
    
    public virtual void OnClose()
    {
        
    }
    
}