using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Models.Documentation;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.Docs;
using SkEditor.Utilities.Docs.Providers;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Tabs;

public partial class DocumentationTabViewModel : TabViewModelBase
{
    private readonly IDocumentationService _documentation;
    private readonly ILogService _logs;
    private readonly INotificationService _notifs;

    private CancellationTokenSource? _cts;

    public List<string?> AvailableAddons => DocumentationEntries?
        .Select(entry => entry.Addon)
        .Where(addon => !string.IsNullOrEmpty(addon))
        .Prepend(null)
        .Distinct()
        .ToList() ?? [];

    public override IRelayCommand CloseCommand => CloseTabCommand;

    public IReadOnlyList<DocumentationEntry>? DocumentationEntries { get; private set; }

    [ObservableProperty]
    public partial List<DocumentationEntry>? FilteredEntries { get; set; }

    public override bool IsCacheable => true;

    public TextDocument PatternDocument { get; set; } = new();

    public List<IDocumentationProvider> Providers { get; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? SelectedAddon { get; set; } = null;

    [ObservableProperty]
    public partial DocumentationEntry? SelectedEntry { get; set; }

    [ObservableProperty]
    public partial IDocumentationProvider? SelectedProvider { get; set; }

    [ObservableProperty]
    public partial DocumentationSyntaxType? SelectedSyntaxType { get; set; }

    public BindableReactiveProperty<bool> IsLoadingDocs { get; } = new(false);

    public List<DocumentationSyntaxType> SyntaxTypes { get; } = [.. Enum.GetValues<DocumentationSyntaxType>()];

    public DocumentationTabViewModel(IDocumentationService documentation, INotificationService notifs, ILogService logs,
        IEnumerable<IDocumentationProvider> providers) : base(TranslationKey.DocumentationTabTitle, Symbol.Book)
    {
        _documentation = documentation;
        _notifs = notifs;
        _logs = logs;

        Providers = [.. providers];

        SelectedProvider ??= Providers.FirstOrDefault();
    }

    public async Task LoadDocs(CancellationToken ct = default)
    {
        IDocumentationProvider? provider = SelectedProvider;
        if (provider is null)
        {
            return;
        }

        try
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            IsLoadingDocs.Value = true;

            string downloadUrl = provider.GetDownloadUrl();

            await using Stream docStream =
                await _documentation.GetDocumentationStream(provider.FileName, downloadUrl, ct);
            IReadOnlyList<DocumentationEntry> entries = await provider.GetDocumentationEntriesAsync(docStream, ct);
            IsLoadingDocs.Value = false;

            stopwatch.Stop();
            _notifs.Debug($"Loading documentation took {stopwatch.ElapsedMilliseconds} ms");

            if (ct.IsCancellationRequested)
            {
                return;
            }

            DocumentationEntries = entries;
            SelectedAddon = null;
            OnPropertyChanged(nameof(AvailableAddons));
            SelectedSyntaxType ??= DocumentationSyntaxType.All;
            Filter();
        }
        catch (DocumentationException ex) when (ex.ApiKeyMissing)
        {
            Notification notification = Notification
                .Create(TranslationKey.DocumentationLoadErrorTitle)
                .WithMessage(TranslationKey.DocumentationApiKeyMissing)
                .WithType(NotificationType.Error);

            _notifs.Show(notification);
        }
        catch (Exception ex)
        {
            Notification notification = Notification
                .Create(TranslationKey.DocumentationLoadErrorTitle)
                .WithMessage(TranslationKey.DocumentationLoadErrorMessage)
                .WithType(NotificationType.Error);

            _notifs.Show(notification);
            _logs.Error(ex, "Error loading documentation");
        }
        finally
        {
            IsLoadingDocs.Value = false;
        }
    }

    [RelayCommand]
    private void CloseTab()
    {
        RequestClose();
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