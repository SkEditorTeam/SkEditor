using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using SkEditor.API;
using SkEditor.Utilities.Extensions;

namespace SkEditor.Utilities.Docs.SkriptHub;

public class SkriptHubProvider : IDocProvider
{
    private const string BaseUri = "https://skripthub.net/api/v1/";

    private readonly List<SkriptHubDocEntry> _cachedElements = [];

    private readonly HttpClient _client = new();

    public DocProvider Provider => DocProvider.SkriptHub;

    public Task<IDocumentationEntry?> FetchElement(string id)
    {
        throw new NotImplementedException();
    }

    public List<string> CanSearch(SearchData searchData)
    {
        if (searchData.Query.Length < 3 && string.IsNullOrEmpty(searchData.FilteredAddon) &&
            searchData.FilteredType == IDocumentationEntry.Type.All)
        {
            return [Translation.Get("DocumentationWindowInvalidDataQuery")];
        }

        return [];
    }

    public async Task<List<IDocumentationEntry>> Search(SearchData searchData)
    {
        const string uri = BaseUri + "addonsyntaxlist/";

        if (_cachedElements.Count > 0)
        {
            List<SkriptHubDocEntry> foundElements = _cachedElements.Where(e => e.DoMatch(searchData)).ToList();
            return foundElements.Cast<IDocumentationEntry>().ToList();
        }

        string cacheFile = Path.Combine(AppConfig.AppDataFolderPath, "SkriptHubCache.json");
        if (!File.Exists(cacheFile))
        {
            TaskDialog taskDialog = new()
            {
                Title = Translation.Get("DocumentationWindowCacheSkriptHub"),
                ShowProgressBar = true,
                IconSource = new SymbolIconSource { Symbol = Symbol.Download },
                SubHeader = Translation.Get("DocumentationWindowDownloading"),
                Content = Translation.Get("DocumentationWindowCacheSkriptHubMessage")
            };

            taskDialog.Opened += async (_, _) =>
            {
                try
                {
                    using HttpClient client = new();
                    Progress<float> progress = new();
                    progress.ProgressChanged += (_, sender) =>
                        taskDialog.SetProgressBarState(sender, TaskDialogProgressState.Normal);

                    await using FileStream file = new(cacheFile, FileMode.Create, FileAccess.Write, FileShare.None);
                    await client.DownloadDataAsync(uri, file, progress);
                    file.Close();

                    taskDialog.Hide(TaskDialogStandardResult.OK);
                }
                catch (Exception e)
                {
                    await SkEditorAPI.Windows.ShowError(e is TaskCanceledException
                        ? Translation.Get("DocumentationWindowErrorOffline")
                        : Translation.Get("DocumentationWindowErrorGlobal", e.Message));
                    _cachedElements.Clear();

                    taskDialog.Hide(TaskDialogStandardResult.Cancel);
                }
            };

            taskDialog.XamlRoot = SkEditorAPI.Windows.GetMainWindow();
            TaskDialogStandardResult result = (TaskDialogStandardResult)await taskDialog.ShowAsync();
            if (result == TaskDialogStandardResult.Cancel)
            {
                return [];
            }
        }

        string content = await File.ReadAllTextAsync(cacheFile);
        List<SkriptHubDocEntry>? elements = JsonConvert.DeserializeObject<List<SkriptHubDocEntry>>(content);
        if (elements == null)
        {
            await SkEditorAPI.Windows.ShowError(Translation.Get("DocumentationWindowErrorGlobal", "Invalid cache file"));
            return [];
        }
        _cachedElements.AddRange(elements);
        await SaveCache();

        List<SkriptHubDocEntry> foundElements2 = _cachedElements.Where(e => e.DoMatch(searchData)).ToList();
        return foundElements2.Cast<IDocumentationEntry>().ToList();
    }

    public bool IsAvailable()
    {
        return true;
    }

    public bool NeedsToLoadExamples => true;

    public async Task<List<IDocumentationExample>> FetchExamples(IDocumentationEntry entry)
    {
        SkriptHubDocEntry? foundEntry = _cachedElements.FirstOrDefault(e => e.Id == entry.Id);
        if (foundEntry == null)
        {
            return [];
        }

        if (foundEntry.Examples != null)
        {
            return foundEntry.Examples.Cast<IDocumentationExample>().ToList();
        }

        string elementId = entry.Id;
        string uri = BaseUri + "syntaxexample/" + "?syntax=" + elementId;

        CancellationTokenSource cancellationToken = new(new TimeSpan(0, 0, 5));
        HttpResponseMessage response;
        try
        {
            response = await _client.GetAsync(uri, cancellationToken.Token);
        }
        catch (Exception e)
        {
            await SkEditorAPI.Windows.ShowError(e is TaskCanceledException
                ? Translation.Get("DocumentationWindowErrorOffline")
                : Translation.Get("DocumentationWindowErrorGlobal", e.Message));
            return [];
        }

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                SkEditorAPI.Logs.Info(await response.Content.ReadAsStringAsync(cancellationToken.Token));
            }
            catch (Exception e)
            {
                SkEditorAPI.Logs.Error(e.Message);
            }

            await SkEditorAPI.Windows.ShowError(
                Translation.Get("DocumentationWindowErrorGlobal", response.ReasonPhrase));
            return [];
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken.Token);
        List<SkriptHubDocExample>? elements = JsonConvert.DeserializeObject<List<SkriptHubDocExample>>(content);
        foundEntry.Examples = elements;
        await SaveCache();
        List<IDocumentationExample>? examples = elements?.Cast<IDocumentationExample>()?.ToList();
        return examples ?? [];
    }

    public bool HasAddons => false;

    public async Task<List<string>> GetAddons()
    {
        if (_cachedElements.Count > 0)
        {
            return _cachedElements.Select(e => e.Addon).Distinct().ToList();
        }

        string cacheFile = Path.Combine(AppConfig.AppDataFolderPath, "SkriptHubCache.json");
        if (!File.Exists(cacheFile))
        {
            return [];
        }

        string content = await File.ReadAllTextAsync(cacheFile);
        List<SkriptHubDocEntry>? elements = JsonConvert.DeserializeObject<List<SkriptHubDocEntry>>(content);
        if (elements == null)
        {
            await SkEditorAPI.Windows.ShowError(Translation.Get("DocumentationWindowErrorGlobal", "Invalid cache file"));
            return [];
        }
        _cachedElements.AddRange(elements);
        await SaveCache();

        return _cachedElements.Select(e => e.Addon).Distinct().ToList();
    }

    public Task<Color?> GetAddonColor(string addonName)
    {
        return Task.FromResult<Color?>(null);
    }

    public IconSource Icon => new ImageIconSource
    {
        Source = new SvgImage
        {
            Source = SvgSource.LoadFromStream(
                AssetLoader.Open(new Uri("avares://SkEditor/Assets/Brands/SkriptHub.svg")))
        }
    };

    public string GetLink(IDocumentationEntry entry)
    {
        return "https://skripthub.net/docs/?id=" + entry.Id;
    }

    public static IDocProvider Get()
    {
        return IDocProvider.Providers[DocProvider.SkriptHub];
    }

    public async Task SaveCache()
    {
        string cacheFile = Path.Combine(AppConfig.AppDataFolderPath, "SkriptHubCache.json");
        string content = JsonConvert.SerializeObject(_cachedElements);
        await File.WriteAllTextAsync(cacheFile, content);
    }

    public void DeleteEverything()
    {
        _cachedElements.Clear();
        File.Delete(Path.Combine(AppConfig.AppDataFolderPath, "SkriptHubCache.json"));
    }
}