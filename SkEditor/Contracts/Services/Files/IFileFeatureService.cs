using System.Collections.Generic;
using SkEditor.Features;
using SkEditor.Models.Tabs;

namespace SkEditor.Contracts.Services.Files;

public interface IFileFeatureService
{
    void OnTabOpened(FileTab tab);

    void OnTabClosed(FileTab tab);

    IEnumerable<IEditorFeature> GetFeaturesForTab(FileTab tab);
}