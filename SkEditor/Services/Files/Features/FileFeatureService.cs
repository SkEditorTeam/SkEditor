using System.Collections.Generic;
using System.Linq;
using AvaloniaEdit;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.Features;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Settings;

namespace SkEditor.Services.Files.Features;

public class FileFeatureService : IFileFeatureService
{
    private readonly Dictionary<FileTab, List<IEditorFeature>> _attachedFeatures = [];
    private readonly Dictionary<FileTab, TextEditor> _activeEditors = [];
    private readonly IEnumerable<IEditorFeature> _allFeatures;

    public FileFeatureService(IEnumerable<IEditorFeature> allFeatures, IEnumerable<IConfig> configs)
    {
        _allFeatures = allFeatures;

        foreach (IConfig config in configs)
        {
            config.PropertyChanged += (_, _) => UpdateAllFeatures();
        }
    }

    public void OnTabOpened(FileTab tab)
    {
        List<IEditorFeature> features = _allFeatures
            .Where(f => f.CanAttach(tab))
            .ToList();

        _attachedFeatures[tab] = features;

        foreach (IEditorFeature feature in features)
        {
            feature.Attach(tab);
        }
    }

    public void OnTabClosed(FileTab tab)
    {
        if (!_attachedFeatures.TryGetValue(tab, out var features))
        {
            return;
        }

        bool hasView = _activeEditors.TryGetValue(tab, out TextEditor? editor);

        foreach (var feature in features)
        {
            if (hasView)
            {
                feature.OnViewDetached(tab, editor!);
            }

            feature.Detach(tab);
        }

        _attachedFeatures.Remove(tab);
        _activeEditors.Remove(tab);
    }

    public IEnumerable<IEditorFeature> GetFeaturesForTab(FileTab tab)
    {
        return _attachedFeatures.TryGetValue(tab, out var features) ? features : [];
    }

    public void UpdateAllFeatures()
    {
        foreach (FileTab tab in _attachedFeatures.Keys)
        {
            UpdateFeatures(tab);
        }
    }

    public void UpdateFeatures(FileTab tab)
    {
        if (!_attachedFeatures.TryGetValue(tab, out List<IEditorFeature>? currentFeatures))
        {
            return;
        }

        HashSet<IEditorFeature> desired = _allFeatures.Where(f => f.CanAttach(tab)).ToHashSet();
        HashSet<IEditorFeature> current = currentFeatures.ToHashSet();

        bool hasView = _activeEditors.TryGetValue(tab, out TextEditor? editor);

        foreach (IEditorFeature feature in current.Except(desired))
        {
            if (hasView)
            {
                feature.OnViewDetached(tab, editor!);
            }

            feature.Detach(tab);
        }

        foreach (IEditorFeature feature in desired.Except(current))
        {
            feature.Attach(tab);
            if (hasView)
            {
                feature.OnViewAttached(tab, editor!);
            }
        }

        _attachedFeatures[tab] = [..desired];
    }

    public void OnViewLoaded(FileTab tab, TextEditor editor)
    {
        _activeEditors[tab] = editor;
        
        if (!_attachedFeatures.TryGetValue(tab, out var features)) return;

        foreach (var feature in features)
        {
            feature.OnViewAttached(tab, editor);
        }
    }

    public void OnViewUnloaded(FileTab tab, TextEditor editor)
    {
        _activeEditors.Remove(tab);
        
        if (!_attachedFeatures.TryGetValue(tab, out var features)) return;
        
        foreach (var feature in features)
        {
            feature.OnViewDetached(tab, editor);
        }
    }
}