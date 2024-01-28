using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Parser;

namespace SkEditor.Controls.Sidebar;

public partial class ParserSidebarPanel : UserControl
{
    public ObservableCollection<CodeSection> Sections { get; set; } = new ();
    
    public void Refresh(List<CodeSection> sections)
    {
        Sections.Clear();
        sections.ForEach(section => Sections.Add(section));
        ItemsRepeater.ItemsSource = Sections;
        UpdateInformationBox();
    }
    
    public ParserSidebarPanel()
    {
        InitializeComponent();
        
        ParseButton.Click += (_, _) => ParseCurrentFile();
    }

    public void ParseCurrentFile()
    {
        var selectedItem = ApiVault.Get().GetTabView().SelectedItem as TabViewItem;
        if (selectedItem == null) 
            return;
        var parser = FileHandler.OpenedFiles.Find(file => file.TabViewItem == selectedItem)?.Parser;
        if (parser == null) 
            return;
            
        ParseButton.IsEnabled = false;
        parser.Parse();
    }

    public void UpdateInformationBox(bool isToNotifyUnParsing = false)
    {
        if (isToNotifyUnParsing)
        {
            Sections.Clear();
            CannotParseInfo.IsVisible = true;
            CannotParseInfoText.Text = "File changed, you need to parse it again.";
            return;
        }
        
        if (Sections.Count == 0)
        {
            CannotParseInfo.IsVisible = true;
            CannotParseInfoText.Text = "No sections found in the code. Maybe you should write something? :)";
            return;
        }

        var parser = Sections[0].Parser;
        if (!parser.IsValid())
        {
            Sections.Clear();
            CannotParseInfo.IsVisible = true;
            CannotParseInfoText.Text = "This file cannot be parsed, as it do not look likes a script file.";
            return;
        }

        CannotParseInfo.IsVisible = false;
    }
    
    public class ParserPanel : SidebarPanel
    {
        public override UserControl Content => Panel;
        public override IconSource Icon => new SymbolIconSource() { Symbol = Symbol.Code };
        public override bool IsDisabled => false;
        
        public readonly ParserSidebarPanel Panel = new ();
        
        public override int DesiredWidth { get; } = 350;
    }
}