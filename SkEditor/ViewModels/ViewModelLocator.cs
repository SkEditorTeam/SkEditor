using SkEditor.API;
using AppSettings = SkEditor.Utilities.Settings.Configs.AppSettings;
using BinarySettings = SkEditor.Utilities.Settings.Configs.BinarySettings;
using StateConfig = SkEditor.Utilities.Settings.Configs.StateConfig;

namespace SkEditor.ViewModels;

public static class ViewModelLocator
{
    public static AppSettings AppSettings => SkEditorAPI.Configs.GetAppSettings();
    public static StateConfig StateConfig => SkEditorAPI.Configs.GetStateConfig();
    public static BinarySettings BinarySettings => SkEditorAPI.Configs.GetBinarySettings();
}