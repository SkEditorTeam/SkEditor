using System.Collections.Generic;
using Avalonia.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.Utilities;
using SkEditor.Utilities.Styling;
using Symbol = FluentIcons.Common.Symbol;

namespace SkEditor.Views.Windows.FileTypes;

public partial class AssociationSelectionWindow : AppWindow
{
    public AssociationSelectionWindow(List<Utilities.Files.FileTypes.FileAssociation> fileTypes)
    {
        InitializeComponent();
        WindowStyler.Style(this);
        Focusable = true;

        fileTypes.Sort((a, b) => a.IsFromAddon.CompareTo(b.IsFromAddon));
        SelectedAssociation = fileTypes.Find(association => !association.IsFromAddon);

        foreach (Utilities.Files.FileTypes.FileAssociation association in fileTypes)
        {
            AssociationItemView item = new()
            {
                Source = association.IsFromAddon ? association.Addon?.Name : "SkEditor",
                Description = association.IsFromAddon
                    ? Translation.Get("FileAssociationSelectionWindowAddonItem", association.Addon?.Name)
                    : Translation.Get("FileAssociationSelectionWindowOfficialItem"),
                Tag = association.IsFromAddon ? association.Addon?.Name : "SkEditor"
            };
            item.UpdateIcon(association.IsFromAddon ? Symbol.Edit : Symbol.Checkmark);
            Associations.Items.Add(item);

            if (!association.IsFromAddon)
            {
                Associations.SelectedItem = item;
            }
        }

        Associations.SelectionChanged += (_, _) =>
        {
            if (Associations.SelectedItem is not AssociationItemView item)
            {
                return;
            }

            SelectedAssociation = fileTypes.Find(association =>
                association.IsFromAddon && association.Addon?.Name == item.Tag?.ToString());
        };

        ConfirmButton.Click += (_, _) => Close();

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };
    }

    public Utilities.Files.FileTypes.FileAssociation? SelectedAssociation { get; set; }
}