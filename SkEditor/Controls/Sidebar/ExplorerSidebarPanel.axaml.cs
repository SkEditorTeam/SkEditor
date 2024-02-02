using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities;
using SkEditor.Utilities.Projects;

namespace SkEditor.Controls.Sidebar;

public partial class ExplorerSidebarPanel : UserControl
{
    public ExplorerSidebarPanel()
    {
        InitializeComponent();
    }
    
    public class ExplorerPanel : SidebarPanel
    {
        public override UserControl Content => Panel;
        public override IconSource Icon => new SymbolIconSource() { Symbol = Symbol.Folder };
        public override bool IsDisabled => false;
        
        public readonly ExplorerSidebarPanel Panel = new ();
    }

    private void OpenFolder(object? sender, RoutedEventArgs e)
    {
        ProjectOpener.OpenProject();
    }
}