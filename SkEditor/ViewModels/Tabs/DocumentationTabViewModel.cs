using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Models.Documentation;
using SkEditor.Utilities.Docs;
using SkEditor.Utilities.Docs.Providers;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Tabs;

public partial class DocumentationTabViewModel : TabViewModelBase
{
    public DocumentationTabViewModel() : base(TranslationKey.DocumentationTabTitle, Symbol.Book) =>
        _ = Task.Run(async () =>
        {
            await LoadDocs();

            SelectedSyntaxType = DocumentationSyntaxType.All;
        });

    public override IRelayCommand CloseCommand => CloseTabCommand;

    public override bool IsCacheable => true;

    public IReadOnlyList<DocumentationEntry>? DocumentationEntries { get; private set; }

    [ObservableProperty]
    public partial List<DocumentationEntry>? FilteredEntries { get; set; }

    [ObservableProperty]
    public partial DocumentationEntry? SelectedEntry { get; set; }

    public List<DocumentationSyntaxType> SyntaxTypes { get; } = Enum.GetValues<DocumentationSyntaxType>().ToList();

    [ObservableProperty]
    public partial DocumentationSyntaxType? SelectedSyntaxType { get; set; }

    private const string AllAddonsValue = "All Addons";

    public List<string> AvailableAddons => DocumentationEntries?
        .Select(entry => entry.Addon)
        .Where(addon => !string.IsNullOrEmpty(addon))
        .Prepend(AllAddonsValue)
        .Distinct()
        .ToList() ?? [];

    [ObservableProperty]
    public partial string SelectedAddon { get; set; } = AllAddonsValue;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    public TextDocument PatternDocument { get; set; } = new();

    [RelayCommand]
    private void CloseTab()
    {
        RequestClose();
    }

    public async Task LoadDocs()
    {
        IDocumentationProvider provider = new SkDocsProvider();
        DocumentationEntries = await provider.GetDocumentationEntriesAsync();

        FilteredEntries = DocumentationEntries.ToList();
    }

    partial void OnSelectedEntryChanged(DocumentationEntry? value)
    {
        PatternDocument.Text = value?.Patterns ?? string.Empty;
    }

    partial void OnSelectedSyntaxTypeChanged(DocumentationSyntaxType? value)
    {
        Filter();
    }

    partial void OnSearchTextChanged(string value)
    {
        Filter();
    }

    partial void OnSelectedAddonChanged(string value)
    {
        Filter();
    }

    private void Filter()
    {
        FilteredEntries = DocumentationSearchEngine.Filter(SearchText, DocumentationEntries,
            SelectedSyntaxType ?? DocumentationSyntaxType.All, SelectedAddon);
    }
}