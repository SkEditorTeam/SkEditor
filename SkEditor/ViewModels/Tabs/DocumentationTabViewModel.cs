using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Models.Documentation;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Tabs;

public partial class DocumentationTabViewModel(HttpClient httpClient, INotificationService notifs)
    : TabViewModelBase(TranslationKey.DocumentationTabTitle, Symbol.Book)
{
    public override IRelayCommand CloseCommand => CloseTabCommand;

    public override bool IsCacheable => true;

    [ObservableProperty]
    public partial List<DocumentationEntry>? DocumentationEntries { get; private set; }

    [RelayCommand]
    private void CloseTab()
    {
        RequestClose();
    }

    [RelayCommand]
    public async Task TestDocs()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Uri jsonFile = new("avares://SkEditor/Assets/skunity-docs.json");
        await using Stream stream = AssetLoader.Open(jsonFile);

        SkUnityResponseDto? response =
            await JsonSerializer.DeserializeAsync<SkUnityResponseDto>(stream,
                SkUnityResponseContext.Default.SkUnityResponseDto);

        if (response is null)
        {
            notifs.Debug("Failed to deserialize SkUnityResponse.");
            return;
        }

        DocumentationEntries = response.ResultDto.Docs.Values.Select(doc => new DocumentationEntry
        {
            Name = doc.Name,
            Type = doc.Type,
            Description = doc.Description,
            Addon = doc.Addon,
            Version = doc.Version,
            Patterns = doc.Pattern.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
        }).ToList();

        stopwatch.Stop();

        notifs.Debug(
            $"docsów: {response.ResultDto.Docs.Count} items, deserialized in {stopwatch.ElapsedMilliseconds}ms.");
    }
}