using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Models.Documentation;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.Docs;
using SkEditor.Utilities.Docs.Providers;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Tabs;

public partial class DocumentationTabViewModel : TabViewModelBase
{
    private readonly ILogService _logs;
    private readonly INotificationService _notifs;

    private CancellationTokenSource? _cts;

    public DocumentationTabViewModel(INotificationService notifs, ILogService logs) : base(
        TranslationKey.DocumentationTabTitle, Symbol.Book)
    {
        _notifs = notifs;
        _logs = logs;

        SelectedProvider ??= Providers.FirstOrDefault();
    }

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

    public List<string?> AvailableAddons => DocumentationEntries?
        .Select(entry => entry.Addon)
        .Where(addon => !string.IsNullOrEmpty(addon))
        .Prepend(null)
        .Distinct()
        .ToList() ?? [];

    [ObservableProperty]
    public partial string? SelectedAddon { get; set; } = null;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    public TextDocument PatternDocument { get; set; } = new();

    public List<IDocumentationProvider> Providers { get; } =
    [
        new SkDocsProvider(),
        new SkUnityProvider()
    ];

    [ObservableProperty]
    public partial IDocumentationProvider? SelectedProvider { get; set; }

    [RelayCommand]
    private void CloseTab()
    {
        RequestClose();
    }

    public async Task LoadDocs(CancellationToken cancellationToken = default)
    {
        IDocumentationProvider? provider = SelectedProvider;
        if (provider is null)
        {
            return;
        }

        try
        {
            IReadOnlyList<DocumentationEntry> entries = await provider.GetDocumentationEntriesAsync();

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            DocumentationEntries = entries;
            SelectedSyntaxType ??= DocumentationSyntaxType.All;
            Filter();
        }
        catch (Exception ex)
        {
            Notification notification = Notification
                .Create("Error loading documentation".ToTranslatable())
                .WithMessage("Something went wrong when loading documentation. Try again or look into the logs."
                    .ToTranslatable())
                .WithType(NotificationType.Error);

            _notifs.Show(notification);
            _logs.Error(ex, "Error loading documentation");
        }
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

    partial void OnSelectedAddonChanged(string? value)
    {
        Filter();
    }

    partial void OnSelectedProviderChanged(IDocumentationProvider? value)
    {
        OnPropertyChanged(nameof(AvailableAddons));

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;

        _ = LoadDocs(token);
    }

    private void Filter()
    {
        FilteredEntries = DocumentationSearchEngine.Filter(SearchText, DocumentationEntries,
            SelectedSyntaxType ?? DocumentationSyntaxType.All, SelectedAddon);
    }
}