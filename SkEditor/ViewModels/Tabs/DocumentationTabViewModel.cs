using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Platform;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Models.Documentation;
using SkEditor.Utilities.Docs;
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
        Uri jsonFile = new("avares://SkEditor/Assets/skunity-docs.json");
        await using Stream stream = AssetLoader.Open(jsonFile);

        SkUnityResponseDto? response =
            await JsonSerializer.DeserializeAsync<SkUnityResponseDto>(stream,
                SkUnityResponseContext.Default.SkUnityResponseDto);

        if (response is null)
        {
            return;
        }

        DocumentationEntries = response.ResultDto.Docs.Values.Select(doc => new DocumentationEntry
        {
            Name = WebUtility.HtmlDecode(doc.Name),
            Type = DocumentationSyntaxType.FromString(doc.Type),
            Description = WebUtility.HtmlDecode(doc.Description),
            Addon = WebUtility.HtmlDecode(doc.Addon),
            Version = WebUtility.HtmlDecode(doc.Version),
            Patterns = WebUtility.HtmlDecode(doc.Pattern)
        }).ToList();

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