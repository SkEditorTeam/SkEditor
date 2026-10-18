using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using SkEditor.Appearance.Icons;
using TranslatableString = SkEditor.Core.Localization.TranslatableString;

namespace SkEditor.Layout.Sidebar.UI.Panes;

public class SearchPaneViewModel : ObservableObject, ISidebarPanelViewModel
{
    public StandardIcon Icon => Symbol.Search;
    public Thickness IconOffset => new(0, -2, 0, 0);
    public TranslatableString Name { get; } = TranslatableString.FromRawString("Search");
}