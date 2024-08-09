using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Utilities.Docs.Local;

public class LocalProvider : IDocProvider
{
    public DocProvider Provider => DocProvider.Local;

    public async Task<IDocumentationEntry> FetchElement(string id)
    {
        if (!IsLoaded)
            await LoadLocalDocs();

        return _localDocs.Find(x => x.Id == id);
    }

    public async Task<List<IDocumentationEntry>> Search(SearchData searchData)
    {
        if (!IsLoaded)
            await LoadLocalDocs();

        return _localDocs.FindAll(x => x.DoMatch(searchData)).Cast<IDocumentationEntry>().ToList();
    }

    public async Task<List<IDocumentationExample>> FetchExamples(IDocumentationEntry entry)
    {
        if (!IsLoaded)
            await LoadLocalDocs();
        var localEntry = entry as LocalDocEntry;

        return (localEntry?.Examples ?? []).Cast<IDocumentationExample>().ToList();
    }

    #region Local File Management

    private List<LocalDocEntry> _localDocs = null!;

    private string GetLocalCachePath()
    {
        var folder = Path.Combine(AppConfig.AppDataFolderPath, "Docs");
        Directory.CreateDirectory(folder);
        var file = Path.Combine(folder, "local.json");
        if (!File.Exists(file))
            File.WriteAllText(file, "[]");
        return file;
    }

    private async Task LoadLocalDocs()
    {
        var file = GetLocalCachePath();
        var content = await File.ReadAllTextAsync(file);

        _localDocs = new();
        _localDocs.AddRange(JsonConvert.DeserializeObject<List<LocalDocEntry>>(content));
    }

    private async Task SaveLocalDocs()
    {
        var file = GetLocalCachePath();
        var content = JsonConvert.SerializeObject(_localDocs);
        await File.WriteAllTextAsync(file, content);
    }

    public async Task<bool> IsElementDownloaded(IDocumentationEntry entry)
    {
        if (!IsLoaded)
            await LoadLocalDocs();

        return _localDocs.Any(x => x.Id == entry.Id);
    }

    public async void DownloadElement(IDocumentationEntry entry, List<IDocumentationExample> examples)
    {
        if (!IsLoaded)
            await LoadLocalDocs();

        if (_localDocs.Any(x => x.Id == entry.Id))
            return;

        _localDocs.Add(new LocalDocEntry(entry, examples));
        await SaveLocalDocs();
    }

    public async void RemoveElement(IDocumentationEntry entry)
    {
        if (!IsLoaded)
            await LoadLocalDocs();

        _localDocs.RemoveAll(x => x.Id == entry.Id);
        await SaveLocalDocs();
    }

    public bool IsLoaded => _localDocs != null!;

    #endregion

    #region Not Supported Stuff

    public bool NeedsToLoadExamples => false;
    public bool HasAddons => false;
    public Task<List<string>> GetAddons()
    {
        throw new NotImplementedException();
    }

    public List<string> CanSearch(SearchData searchData)
    {
        return [];
    }

    public bool IsAvailable()
    {
        return true;
    }

    #endregion

    public static LocalProvider Get() => IDocProvider.Providers[DocProvider.Local] as LocalProvider;

    public async Task<List<LocalDocEntry>> GetElements()
    {
        if (!IsLoaded)
            await LoadLocalDocs();

        return _localDocs;
    }

    public Task<Color?> GetAddonColor(string addonName) => Task.FromResult<Color?>(null);

    public async Task DeleteAll()
    {
        _localDocs.Clear();
        await SaveLocalDocs();
    }

    public IconSource Icon => new SymbolIconSource() { Symbol = Symbol.Folder, IconVariant = IconVariant.Filled };

    public string? GetLink(IDocumentationEntry entry)
    {
        return null;
    }
}