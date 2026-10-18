using System.Collections.Generic;
using AvaloniaEdit;
using SkEditor.Editor.Tabs.Capabilities;

namespace SkEditor.Editor.Features;

public interface IFileFeatureService
{
    void OnTabOpened(IFeatureTab tab);

    void OnTabClosed(IFeatureTab tab);

    IEnumerable<IEditorFeature> GetFeaturesForTab(IFeatureTab tab);
    void UpdateAllFeatures();
    void UpdateFeatures(IFeatureTab tab);

    void OnViewLoaded(IFeatureTab tab, TextEditor editor);
    void OnViewUnloaded(IFeatureTab tab, TextEditor editor);
}