using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Parser;
using SkEditor.Utilities.Parser.ViewModels;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;
using Symbol = FluentIcons.Common.Symbol;

namespace SkEditor.Views.Controls.Sidebar;

public partial class ParserSidebarPanel : UserControl
{
    public ParserSidebarPanel()
    {
        InitializeComponent();

        DataContext = new ParserFilterViewModel();

        ParserDisabled.IsVisible = !CodeParserEnabled;
        ScrollViewer.IsVisible = CodeParserEnabled;

        if (SkEditorAPI.Core.GetAppConfig().EnableRealtimeCodeParser)
        {
            ParseButton.IsVisible = false;
        }
        else
        {
            ParseButton.IsEnabled = CodeParserEnabled;
            ParseButton.Click += (_, _) => ParseCurrentFile();
        }

        EnableParser.Click += (_, _) =>
        {
            SkEditorAPI.Core.GetAppConfig().EnableCodeParser = true;
            SkEditorAPI.Core.GetAppConfig().Save();

            ParserDisabled.IsVisible = false;
            ScrollViewer.IsVisible = true;
            ParseButton.IsEnabled = true;
        };
        ClearSearch.Click += (_, _) => ClearSearchFilter();
        SearchBox.KeyUp += (_, _) => UpdateSearchFilter();
        TypeFilterBox.SelectionChanged += (_, _) => UpdateSearchFilter();
        SkEditorAPI.Events.OnTabChanged += (_, _) =>
        {
            CodeParser? parser = SkEditorAPI.Files.GetCurrentOpenedFile()?.Parser;
            if (parser == null) return;

            parser.Parse();
        };
    }

    public static bool CodeParserEnabled => SkEditorAPI.Core.GetAppConfig().EnableCodeParser;
    public ObservableCollection<CodeSection> Sections { get; set; } = [];

    public void Refresh(List<CodeSection> sections)
    {
        Sections.Clear();
        sections.ForEach(Sections.Add);

        ParserFilterViewModel? viewModel = (ParserFilterViewModel?)DataContext;
        if (viewModel == null) return;
        
        List<CodeSection> filteredSections = Sections
            .Where(section =>
                string.IsNullOrWhiteSpace(viewModel.SearchText) || section.Name.Contains(viewModel.SearchText))
            .ToList();
        if (viewModel.SelectedFilterIndex != 0)
        {
            CodeSection.SectionType type = viewModel.SelectedFilterIndex switch
            {
                1 => CodeSection.SectionType.Function,
                2 => CodeSection.SectionType.Event,
                3 => CodeSection.SectionType.Options,
                4 => CodeSection.SectionType.Command,
                _ => throw new NotImplementedException()
            };
            filteredSections = filteredSections.Where(section => section.Type == type).ToList();
        }

        ItemsRepeater.ItemsSource = filteredSections;

        UpdateInformationBox();
    }

    public void ParseCurrentFile()
    {
        CodeParser? parser = SkEditorAPI.Files.GetCurrentOpenedFile()?.Parser;
        if (parser == null)
        {
            return;
        }

        ParseButton.IsEnabled = false;
        parser.Parse();
    }

    public void UpdateSearchFilter()
    {
        Refresh([.. Sections]);
    }

    public void ClearSearchFilter()
    {
        ParserFilterViewModel? viewModel = (ParserFilterViewModel?)DataContext;
        if (viewModel == null) return;
        
        viewModel.SearchText = "";
        viewModel.SelectedFilterIndex = 0;
        Refresh([.. Sections]);
    }

    public void UpdateInformationBox(bool notifyUnparsing = false)
    {
        if (!CodeParserEnabled)
        {
            Sections.Clear();
            CannotParseInfoText.IsVisible = false;
            return;
        }

        if (notifyUnparsing)
        {
            Sections.Clear();
            return;
        }

        if (Sections.Count == 0)
        {
            CannotParseInfo.IsVisible = true;
            CannotParseInfoText.Text = Translation.Get("CodeParserNoSections");
            return;
        }

        CodeParser parser = Sections[0].Parser;
        if (!parser.IsValid())
        {
            Sections.Clear();
            CannotParseInfo.IsVisible = true;
            CannotParseInfoText.Text = Translation.Get("CodeParserBadFormat");
            return;
        }

        CannotParseInfo.IsVisible = false;
    }

    public class ParserPanel : SidebarPanel
    {
        public readonly ParserSidebarPanel Panel = new();
        public override UserControl Content => Panel;
        public override IconSource Icon => new SymbolIconSource { Symbol = Symbol.Code };

        public override IconSource IconActive => new SymbolIconSource
            { Symbol = Symbol.Code, IconVariant = IconVariant.Filled };

        public override bool IsDisabled => false;

        public override int DesiredWidth { get; } = 350;
    }
}