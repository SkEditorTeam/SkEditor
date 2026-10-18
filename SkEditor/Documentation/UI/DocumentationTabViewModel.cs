using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Documentation;
using SkEditor.Models.Messages;
using SkEditor.Utilities.Docs;
using SkEditor.Utilities.Docs.Providers;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Tabs;

public partial class DocumentationTabViewModel : TabViewModelBase
{
    private readonly IDocumentationService _documentation;
    private readonly ILogService _logs;
    private readonly INotificationService _notifs;
    private readonly ITabService _tabs;
    private readonly Func<SettingsTabViewModel> _settingsTabFactory;
    private readonly IMessenger _messenger;

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

    [ObservableProperty]
    public partial DocumentationState CurrentState { get; set; } = DocumentationState.Normal;

    [ObservableProperty]
    public partial DocumentationErrorInfo? CurrentError { get; set; }

    [ObservableProperty]
    public partial bool CanChangeFilters { get; set; } = true;

    public List<DocumentationSyntaxType> SyntaxTypes { get; } = [.. Enum.GetValues<DocumentationSyntaxType>()];

    public DocumentationTabViewModel(IDocumentationService documentation, INotificationService notifs, ILogService logs,
        IEnumerable<IDocumentationProvider> providers, ITabService tabs,
        Func<SettingsTabViewModel> settingsTabFactory, IMessenger messenger) : base(
        TranslationKey.DocumentationTabTitle, Symbol.Book)
    {
        _documentation = documentation;
        _notifs = notifs;
        _logs = logs;
        _tabs = tabs;
        _settingsTabFactory = settingsTabFactory;
        _messenger = messenger;

        Providers = [.. providers];

        SelectedProvider ??= Providers.FirstOrDefault();
    }

    public async Task LoadDocs(CancellationToken ct = default)
    {
        IDocumentationProvider? provider = SelectedProvider;
        if (provider is null)
        {
            CurrentState = DocumentationState.Error;
            CurrentError = new DocumentationErrorInfo(
                TranslationKey.DocumentationConnectionErrorGenericTitle,
                TranslationKey.DocumentationConnectionErrorGenericMessage,
                Symbol.Warning,
                false
            );
            
            return;
        }

        using CancellationTokenSource delayCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _ = ShowLoadingWithDelayAsync(delayCts.Token);

        try
        {
            CanChangeFilters = false;
            CurrentError = null;
            
            string downloadUrl = provider.GetDownloadUrl();

            await using Stream docStream =
                await _documentation.GetDocumentationStream(provider.FileName, downloadUrl, ct);
            IReadOnlyList<DocumentationEntry> entries = await provider.GetDocumentationEntriesAsync(docStream, ct);

            if (ct.IsCancellationRequested)
            {
                return;
            }

            DocumentationEntries = entries;
            SelectedAddon = null;
            OnPropertyChanged(nameof(AvailableAddons));
            SelectedSyntaxType ??= DocumentationSyntaxType.All;
            Filter();

            CurrentState = DocumentationEntries.Count == 0
                ? DocumentationState.Empty
                : DocumentationState.Normal;
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (DocumentationException ex) when (ex.ApiKeyMissing || ex.ApiKeyInvalid)
        {
            _documentation.DeleteCache(provider.FileName);
            
            CurrentState = DocumentationState.Error;
            CurrentError = new DocumentationErrorInfo(
                ex.ApiKeyInvalid
                    ? TranslationKey.DocumentationApiKeyInvalidTitle
                    : TranslationKey.DocumentationApiKeyMissingTitle,
                ex.ApiKeyInvalid
                    ? TranslationKey.DocumentationApiKeyInvalidMessage
                    : TranslationKey.DocumentationApiKeyMissingMessage,
                Symbol.Key,
                true,
                true
            );
        }
        catch (HttpRequestException)
        {
            CurrentState = DocumentationState.Error;
            CurrentError = new DocumentationErrorInfo(
                TranslationKey.DocumentationConnectionErrorTitle,
                TranslationKey.DocumentationConnectionErrorMessage,
                Symbol.CloudOff
            );
        }
        catch (Exception ex)
        {
            CurrentState = DocumentationState.Error;
            CurrentError = new DocumentationErrorInfo(
                TranslationKey.DocumentationConnectionErrorGenericTitle,
                TranslationKey.DocumentationConnectionErrorGenericMessage,
                Symbol.Warning
            );
            _logs.Error(ex, "Error loading documentation");
        }
        finally
        {
            await delayCts.CancelAsync();
            CanChangeFilters = true;
        }
    }

    private async Task ShowLoadingWithDelayAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(150, token);
            CurrentState = DocumentationState.Loading;
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    }

    [RelayCommand]
    private void CloseTab()
    {
        RequestClose();
    }

    [RelayCommand]
    public void RetryLoad()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        _ = LoadDocs(_cts.Token);
    }

    [RelayCommand]
    public void OpenApiKeysSettings()
    {
        _tabs.OpenUniqueTab(_settingsTabFactory);
        _messenger.Send(new NavigateToSettingsSectionMessage("API Keys"));
    }

    [RelayCommand]
    public async Task UpdateProvider(IDocumentationProvider provider)
    {
        bool success = await _documentation.DownloadDocumentation(provider.FileName, provider.GetDownloadUrl());

        if (!success)
        {
            CurrentState = DocumentationState.Error;
            CurrentError = new DocumentationErrorInfo(
                TranslationKey.DocumentationConnectionErrorTitle,
                TranslationKey.DocumentationConnectionErrorMessage,
                Symbol.CloudOff
            );
        }
        else if (SelectedProvider == provider)
        {
            await LoadDocs();
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
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;

        _ = LoadDocs(token);
    }

    private void Filter()
    {
        if (DocumentationEntries is null)
        {
            return;
        }

        FilteredEntries = DocumentationSearchEngine.Filter(SearchText, DocumentationEntries,
            SelectedSyntaxType ?? DocumentationSyntaxType.All, SelectedAddon);
    }
}