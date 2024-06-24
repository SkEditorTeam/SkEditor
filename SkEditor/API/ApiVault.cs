using SkEditor.Utilities.Files;
using System;

namespace SkEditor.API;

[Obsolete("Use SkEditorAPI interfaces instead")]
public static class ApiVault
{
    private static ISkEditorAPI instance;

    public static void Set(ISkEditorAPI api)
    {
        instance = api;
    }

    public static ISkEditorAPI Get()
    {
        return instance;
    }

    public static void RegisterFileAssociation(FileTypes.FileAssociation association)
    {
        FileTypes.RegisterExternalAssociation(association);
    }
}