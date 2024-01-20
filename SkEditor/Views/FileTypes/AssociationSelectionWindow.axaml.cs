using System.Collections.Generic;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using SkEditor.Utilities;
using SkEditor.Utilities.Styling;

namespace SkEditor.Views.FileTypes;

public partial class AssociationSelectionWindow : AppWindow
{
    
    public Utilities.Files.FileTypes.FileAssociation? SelectedAssociation { get; set; }
    
    public AssociationSelectionWindow(string path, 
        List<Utilities.Files.FileTypes.FileAssociation> fileTypes)
    {
        InitializeComponent();
        WindowStyler.Style(this);
        
        fileTypes.Sort((a, b) => a.IsFromAddon.CompareTo(b.IsFromAddon));
        SelectedAssociation = fileTypes.Find(association => !association.IsFromAddon);

        foreach (var association in fileTypes)
        {
            var item = new AssociationItemView
            {
                Source = association.IsFromAddon ? association.Addon.Name : "SkEditor",
                Description = association.IsFromAddon 
                    ? Translation.Get("FileAssociationSelectionWindowAddonItem", association.Addon.Name) 
                    : Translation.Get("FileAssociationSelectionWindowOfficialItem"),
                Tag = association.IsFromAddon ? association.Addon.Name : "SkEditor"
            };
            item.UpdateIcon(association.IsFromAddon ? Symbol.Edit : Symbol.Checkmark);
            Associations.Items.Add(item);
            
            if (!association.IsFromAddon)
                Associations.SelectedItem = item;
        }
        
        Associations.SelectionChanged += (_, _) =>
        {
            if (Associations.SelectedItem is not AssociationItemView item)
                return;
            
            SelectedAssociation = fileTypes.Find(association => association.IsFromAddon && association.Addon.Name == item.Tag.ToString());
        };
        
        ConfirmButton.Click += (_, _) => Close();
    }
    
}