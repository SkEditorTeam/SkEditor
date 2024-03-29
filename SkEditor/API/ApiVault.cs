﻿using SkEditor.Utilities.Files;

namespace SkEditor.API;

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