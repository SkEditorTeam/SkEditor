using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities;

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
}