using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
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

        OpenFolderButton.Command = new AsyncRelayCommand(async () => await ProjectOpener.OpenProject());
    }

    public class ExplorerPanel : SidebarPanel
    {
        public readonly ExplorerSidebarPanel Panel = new();
        public override UserControl Content => Panel;
        public override IconSource Icon => new SymbolIconSource { Symbol = Symbol.Folder };

        public override IconSource IconActive => new SymbolIconSource
            { Symbol = Symbol.Folder, IconVariant = IconVariant.Filled };

        public override bool IsDisabled => false;
    }
}