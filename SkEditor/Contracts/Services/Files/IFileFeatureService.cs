using System.Collections.Generic;
using AvaloniaEdit;
using SkEditor.Features;
using SkEditor.Models.Tabs;

namespace SkEditor.Contracts.Services.Files;

public interface IFileFeatureService
{
    void OnTabOpened(FileTab tab);

    void OnTabClosed(FileTab tab);

    IEnumerable<IEditorFeature> GetFeaturesForTab(FileTab tab);

    void OnViewLoaded(FileTab tab, TextEditor editor);
    void OnViewUnloaded(FileTab tab, TextEditor editor);
}