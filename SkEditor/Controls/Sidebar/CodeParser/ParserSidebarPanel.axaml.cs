using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Parser;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Input;
using SkEditor.Utilities.Parser.ViewModels;

namespace SkEditor.Controls.Sidebar;

public partial class ParserSidebarPanel : UserControl
{
    public static bool CodeParserEnabled => ApiVault.Get().GetAppConfig().EnableCodeParser;
    public ObservableCollection<CodeSection> Sections { get; set; } = [];

    public void Refresh(List<CodeSection> sections)
    {
        Sections.Clear();
        sections.ForEach(Sections.Add);
        
        var viewModel = (ParserFilterViewModel) DataContext;
        var filteredSections = Sections
            .Where(section => string.IsNullOrWhiteSpace(viewModel.SearchText) || section.Name.Contains(viewModel.SearchText))
            .ToList();
        if (viewModel.SelectedFilterIndex != 0)
        {
            var type = viewModel.SelectedFilterIndex switch
            {
                1 => CodeSection.SectionType.Function,
                2 => CodeSection.SectionType.Event,
                3 => CodeSection.SectionType.Options,
                4 => CodeSection.SectionType.Command,
                _ => throw new System.NotImplementedException()
            };
            filteredSections = filteredSections.Where(section => section.Type == type).ToList();
        }
        ItemsRepeater.ItemsSource = filteredSections;
        
        UpdateInformationBox();
    }

    public ParserSidebarPanel()
    {
        InitializeComponent();

        DataContext = new ParserFilterViewModel();

        ParserDisabled.IsVisible = !CodeParserEnabled;
        ScrollViewer.IsVisible = CodeParserEnabled;
        ParseButton.IsEnabled = CodeParserEnabled;

        ParseButton.Click += (_, _) => ParseCurrentFile();
        EnableParser.Click += async (_, _) =>
        {
            var response = await ApiVault.Get().ShowMessageWithIcon("Enable Code Parser?", "The code parser let you navigate easily in your code and rename variables, options, and more.\n\nKeep in mind it is still in beta and may BREAK your scripts, so make sure to make a backup before that.",
                new SymbolIconSource() { Symbol = Symbol.Alert });

            if (response == ContentDialogResult.Primary)
            {
                ApiVault.Get().GetAppConfig().EnableCodeParser = true;
                ApiVault.Get().GetAppConfig().Save();

                ParserDisabled.IsVisible = false;
                ScrollViewer.IsVisible = true;
                ParseButton.IsEnabled = true;
            }
        };
        ClearSearch.Click += (_, _) => ClearSearchFilter();
        SearchBox.KeyUp += (_, _) => UpdateSearchFilter();
        TypeFilterBox.SelectionChanged += (_, _) => UpdateSearchFilter();
    }

    public void ParseCurrentFile()
    {
        if (ApiVault.Get().GetTabView().SelectedItem is not TabViewItem selectedItem) return;

        var parser = FileHandler.OpenedFiles.Find(file => file.TabViewItem == selectedItem)?.Parser;
        if (parser == null) return;

        ParseButton.IsEnabled = false;
        parser.Parse();
    }

    public void UpdateSearchFilter()
    {
        Refresh(Sections.ToList());
    }

    public void ClearSearchFilter()
    {
        var viewModel = (ParserFilterViewModel) DataContext;
        viewModel.SearchText = "";
        viewModel.SelectedFilterIndex = 0;
        Refresh(Sections.ToList());
    }

    public void UpdateInformationBox(bool isToNotifyUnParsing = false)
    {
        if (!CodeParserEnabled)
        {
            Sections.Clear();
            CannotParseInfoText.IsVisible = false;
            return;
        }

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

        public readonly ParserSidebarPanel Panel = new();

        public override int DesiredWidth { get; } = 350;
    }
}