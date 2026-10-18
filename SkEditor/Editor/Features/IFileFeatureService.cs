using System.Collections.Generic;
using AvaloniaEdit;
using SkEditor.Features;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Contracts.Services.Files.Features;

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