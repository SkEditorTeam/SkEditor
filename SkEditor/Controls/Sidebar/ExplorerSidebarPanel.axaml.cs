using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;
using SkEditor.Utilities;
using SkEditor.Utilities.Projects;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;
using Symbol = FluentIcons.Common.Symbol;

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
        public override IconSource IconActive => new SymbolIconSource() { Symbol = Symbol.Folder, IconVariant = IconVariant.Filled };
        public override bool IsDisabled => false;

        public readonly ExplorerSidebarPanel Panel = new();
    }

    private void OpenFolder(object? sender, RoutedEventArgs e)
    {
        ProjectOpener.OpenProject();
    }
}