using SkEditor.API;
using SkEditor.Utilities.Settings;

namespace SkEditor.ViewModels;

public static class ViewModelLocator
{
    public static AppConfig AppConfig => SkEditorAPI.Core.GetAppConfig();
    public static BinarySettings? BinarySettings => App.BinarySettings;
}