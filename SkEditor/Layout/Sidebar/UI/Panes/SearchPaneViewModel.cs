using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using SkEditor.Models.UI;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Controls.Sidebar.Panes;

public class SearchPaneViewModel : ObservableObject, ISidebarPanelViewModel
{
    public StandardIcon Icon => Symbol.Search;
    public Thickness IconOffset => new(0, -2, 0, 0);
    public TranslatableString Name { get; } = TranslatableString.FromRawString("Search");
}