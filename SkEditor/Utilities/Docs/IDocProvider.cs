using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Docs.Local;
using SkEditor.Utilities.Docs.SkriptHub;
using SkEditor.Utilities.Docs.SkriptMC;
using SkEditor.Utilities.Docs.SkUnity;

namespace SkEditor.Utilities.Docs;

public interface IDocProvider
{
    static readonly Dictionary<DocProvider, IDocProvider> Providers = new()
    {
        { DocProvider.SkriptHub, new SkriptHubProvider() },
        { DocProvider.skUnity, new SkUnityProvider() },
        { DocProvider.SkriptMC, new SkriptMcProvider() },
        { DocProvider.Local, new LocalProvider() }
    };

    DocProvider Provider { get; }
    string Name => Provider.ToString();

    bool NeedsToLoadExamples { get; }

    bool HasAddons { get; }

    IconSource Icon { get; }

    Task<IDocumentationEntry?> FetchElement(string id);
    Task<List<IDocumentationEntry>> Search(SearchData searchData);

    List<string> CanSearch(SearchData searchData);
    bool IsAvailable();
    Task<List<IDocumentationExample>> FetchExamples(IDocumentationEntry entry);
    Task<List<string>> GetAddons();

    Task<Color?> GetAddonColor(string addonName);
    string? GetLink(IDocumentationEntry entry);
}