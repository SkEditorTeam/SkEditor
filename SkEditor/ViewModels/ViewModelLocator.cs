using SkEditor.API;

namespace SkEditor.ViewModels;

public static class ViewModelLocator
{
    public static object AppConfig => SkEditorAPI.Core.GetAppConfig();
}