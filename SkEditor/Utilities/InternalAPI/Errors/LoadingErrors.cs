using NuGet.Versioning;
using SkEditor.API;
using System;

namespace SkEditor.Utilities.InternalAPI.Classes;

public static class LoadingErrors
{
    public static IAddonLoadingError OutdatedSkEditor(Version minimalVersion) => new OutdatedSkEditorError(minimalVersion);

    public static IAddonLoadingError OutdatedAddon(Version maximalVersion) => new OutdatedAddonError(maximalVersion);

    public static IAddonLoadingError LoadingException(Exception exception) => new LoadingExceptionError(exception);

    public static IAddonLoadingError OutdatedDependency(string dependencyName, NuGetVersion target, NuGetVersion found) => new OutdatedDependencyError(dependencyName, target, found);

    public static IAddonLoadingError MissingAddonDependency(string addonIdentifier)
        => new MissingAddonDependencyError(addonIdentifier);

    public static IAddonLoadingError FailedToLoadDependency(string dependencyName, string message)
        => new FailedToLoadDependencyError(dependencyName, message);


    class OutdatedSkEditorError(Version minimalVersion) : IAddonLoadingError
    {
        public bool IsCritical => true;
        public string Message => "The addon requires a newer version of SkEditor (" + minimalVersion + " and you have " + SkEditorAPI.Core.GetAppVersion() + ").";
    }

    class OutdatedAddonError(Version maximalVersion) : IAddonLoadingError
    {
        public bool IsCritical => true;
        public string Message => "The addon requires a newer version of itself (" + maximalVersion + " and you have " + SkEditorAPI.Core.GetAppVersion() + ").";
    }

    class LoadingExceptionError(Exception exception) : IAddonLoadingError
    {
        public bool IsCritical => true;
        public string Message => "An exception occurred while loading the addon: " + exception.Message;
    }

    class MissingAddonDependencyError(string addonIdentifier) : IAddonLoadingError
    {
        public bool IsCritical => true;
        public string Message => "The addon requires the addon '" + addonIdentifier + "' to be loaded.";
    }

    class OutdatedDependencyError(string dependencyName, NuGetVersion target, NuGetVersion found) : IAddonLoadingError
    {
        public bool IsCritical => true;
        public string Message => "The addon requires the dependency '" + dependencyName + "' to be at least version " + target + ", but you have version " + found + ".";
    }

    class FailedToLoadDependencyError(string dependencyName, string message) : IAddonLoadingError
    {
        public bool IsCritical => true;
        public string Message => "Failed to load the dependency '" + dependencyName + "': " + message;
    }
}