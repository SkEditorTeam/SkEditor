using System.Collections.Generic;
using System.Linq;
using AvaloniaEdit;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Features;
using SkEditor.Models.Tabs;

namespace SkEditor.Services.Files;

public class FileFeatureService(IEnumerable<IEditorFeature> allFeatures, INotificationService notifications)
    : IFileFeatureService
{
    private readonly Dictionary<FileTab, List<IEditorFeature>> _attachedFeatures = [];

    public void OnTabOpened(FileTab tab)
    {
        _attachedFeatures[tab] = [];

        foreach (var feature in allFeatures.Where(f => f.CanAttach(tab)))
        {
            feature.Attach(tab);
            _attachedFeatures[tab].Add(feature);
        }
    }

    public void OnTabClosed(FileTab tab)
    {
        if (!_attachedFeatures.TryGetValue(tab, out var features))
        {
            return;
        }

        foreach (var feature in features)
        {
            feature.Detach(tab);
        }

        _attachedFeatures.Remove(tab);
    }

    public IEnumerable<IEditorFeature> GetFeaturesForTab(FileTab tab)
    {
        return _attachedFeatures.TryGetValue(tab, out var features) ? features : [];
    }

    public void OnViewLoaded(FileTab tab, TextEditor editor)
    {
        if (!_attachedFeatures.TryGetValue(tab, out var features)) return;

        foreach (var feature in features)
        {
            feature.OnViewAttached(tab, editor);
        }
    }

    public void OnViewUnloaded(FileTab tab, TextEditor editor)
    {
        if (!_attachedFeatures.TryGetValue(tab, out var features)) return;
        foreach (var feature in features)
        {
            feature.OnViewDetached(tab, editor);
        }
    }
}