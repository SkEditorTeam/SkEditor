using System.Collections.Generic;
using AvaloniaEdit;
using SkEditor.Features;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Contracts.Services.Files.Features;

public interface IFileFeatureService
{
    void OnTabOpened(FileTabViewModel tab);

    void OnTabClosed(FileTabViewModel tab);

    IEnumerable<IEditorFeature> GetFeaturesForTab(FileTabViewModel tab);
    void UpdateAllFeatures();
    void UpdateFeatures(FileTabViewModel tab);

    void OnViewLoaded(FileTabViewModel tab, TextEditor editor);
    void OnViewUnloaded(FileTabViewModel tab, TextEditor editor);
}