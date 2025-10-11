using System;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using SkEditor.Utilities;
using SkEditor.Utilities.Projects;
using SkEditor.Utilities.Projects.Elements;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;
using Symbol = FluentIcons.Common.Symbol;

namespace SkEditor.Views.Controls.Sidebar;

public partial class ExplorerSidebarPanel : UserControl
{
    public ExplorerSidebarPanel()
    {
        InitializeComponent();

        OpenFolderButton.Command = new AsyncRelayCommand(async () => await ProjectOpener.OpenProject());

        AddSelectedCommandBinding(Key.F2, KeyModifiers.None, e => e.RenameCommand);
        AddSelectedCommandBinding(Key.Delete, KeyModifiers.None, e => e.DeleteCommand);
        AddSelectedCommandBinding(Key.N, KeyModifiers.Control, e => e.CreateNewFileCommand);
        AddSelectedCommandBinding(Key.N, KeyModifiers.Control | KeyModifiers.Shift, e => e.CreateNewFolderCommand);
    }

    private void AddSelectedCommandBinding(Key key, KeyModifiers modifiers,
        Func<StorageElement, IRelayCommand?> getCommand)
    {
        FileTreeView.KeyBindings.Add(new KeyBinding
        {
            Gesture = new KeyGesture(key, modifiers),
            Command = new RelayCommand(() =>
            {
                if (FileTreeView.SelectedItem is StorageElement selected)
                {
                    getCommand(selected)?.Execute(null);
                }
            })
        });
    }

    public class ExplorerPanel : SidebarPanel
    {
        public readonly ExplorerSidebarPanel Panel = new();
        public override UserControl Content => Panel;
        public override IconSource Icon => new SymbolIconSource { Symbol = Symbol.Folder };

        public override IconSource IconActive =>
            new SymbolIconSource { Symbol = Symbol.Folder, IconVariant = IconVariant.Filled };

        public override bool IsDisabled => false;
    }
}