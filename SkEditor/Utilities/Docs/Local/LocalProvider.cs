using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using Newtonsoft.Json;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Utilities.Docs.Local;

public class LocalProvider : IDocProvider
{
    public DocProvider Provider => DocProvider.Local;

    public async Task<IDocumentationEntry?> FetchElement(string id)
    {
        if (!IsLoaded)
        {
            await LoadLocalDocs();
        }

        return _localDocs.Find(x => x.Id == id);
    }

    public async Task<List<IDocumentationEntry>> Search(SearchData searchData)
    {
        if (!IsLoaded)
        {
            await LoadLocalDocs();
        }

        return _localDocs.FindAll(x => x.DoMatch(searchData)).Cast<IDocumentationEntry>().ToList();
    }

    public async Task<List<IDocumentationExample>> FetchExamples(IDocumentationEntry entry)
    {
        if (!IsLoaded)
        {
            await LoadLocalDocs();
        }

        LocalDocEntry? localEntry = entry as LocalDocEntry;

        return (localEntry?.Examples ?? []).Cast<IDocumentationExample>().ToList();
    }

    public Task<Color?> GetAddonColor(string addonName)
    {
        return Task.FromResult<Color?>(null);
    }

    public IconSource Icon => new SymbolIconSource { Symbol = Symbol.Folder, IconVariant = IconVariant.Filled };

    public string? GetLink(IDocumentationEntry entry)
    {
        return null;
    }

    public static LocalProvider? Get()
    {
        return IDocProvider.Providers[DocProvider.Local] as LocalProvider;
    }

    public async Task<List<LocalDocEntry>> GetElements()
    {
        if (!IsLoaded)
        {
            await LoadLocalDocs();
        }

        return _localDocs;
    }

    public async Task DeleteAll()
    {
        _localDocs.Clear();
        await SaveLocalDocs();
    }

    #region Local File Management

    private List<LocalDocEntry> _localDocs = null!;

    private string GetLocalCachePath()
    {
        string folder = Path.Combine(AppConfig.AppDataFolderPath, "Docs");
        Directory.CreateDirectory(folder);
        string file = Path.Combine(folder, "local.json");
        if (!File.Exists(file))
        {
            File.WriteAllText(file, "[]");
        }

        return file;
    }

    private async Task LoadLocalDocs()
    {
        string file = GetLocalCachePath();
        string content = await File.ReadAllTextAsync(file);

        _localDocs = [];
        
        var localDocs = JsonConvert.DeserializeObject<List<LocalDocEntry>>(content) ?? [];
        _localDocs.AddRange(localDocs);
    }

    private async Task SaveLocalDocs()
    {
        string file = GetLocalCachePath();
        string content = JsonConvert.SerializeObject(_localDocs);
        await File.WriteAllTextAsync(file, content);
    }

    public async Task<bool> IsElementDownloaded(IDocumentationEntry entry)
    {
        if (!IsLoaded)
        {
            await LoadLocalDocs();
        }

        return _localDocs.Any(x => x.Id == entry.Id);
    }

    public async Task DownloadElement(IDocumentationEntry entry, List<IDocumentationExample> examples)
    {
        if (!IsLoaded)
        {
            await LoadLocalDocs();
        }

        if (_localDocs.Any(x => x.Id == entry.Id))
        {
            return;
        }

        _localDocs.Add(new LocalDocEntry(entry, examples));
        await SaveLocalDocs();
    }

    public async Task RemoveElement(IDocumentationEntry entry)
    {
        if (!IsLoaded)
        {
            await LoadLocalDocs();
        }

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
}