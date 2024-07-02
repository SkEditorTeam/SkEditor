using NuGet.Versioning;
using SkEditor.API;
using System;

namespace SkEditor.Utilities.InternalAPI.Classes;

public static class LoadingErrors
{

    public static IAddonLoadingError OutdatedSkEditor(Version minimalVersion)
    {
        return new OutdatedSkEditorError(minimalVersion);
    }

    public static IAddonLoadingError OutdatedAddon(Version maximalVersion)
    {
        return new OutdatedAddonError(maximalVersion);
    }

    public static IAddonLoadingError LoadingException(Exception exception)
    {
        return new LoadingExceptionError(exception);
    }

    public static IAddonLoadingError OutdatedDependency(string dependencyName, NuGetVersion target, NuGetVersion found)
    {
        return new OutdatedDependencyError(dependencyName, target, found);
    }

    public static IAddonLoadingError MissingAddonDependency(string addonIdentifier)
    {
        return new MissingAddonDependencyError(addonIdentifier);
    }

    public static IAddonLoadingError FailedToLoadDependency(string dependencyName, string message)
    {
        return new FailedToLoadDependencyError(dependencyName, message);
    }

    // --------------------------------------------------------------------------------------------------------

    class OutdatedSkEditorError : IAddonLoadingError
    {
        private Version MinimalVersion { get; }
        public OutdatedSkEditorError(Version minimalVersion)
        {
            MinimalVersion = minimalVersion;
        }

        public bool IsCritical => true;
        public string Message => "The addon requires a newer version of SkEditor (" + MinimalVersion + " and you have " + SkEditorAPI.Core.GetAppVersion() + ").";
    }
    class OutdatedAddonError : IAddonLoadingError
    {
        private Version MaximalVersion { get; }
        public OutdatedAddonError(Version maximalVersion)
        {
            MaximalVersion = maximalVersion;
        }

        public bool IsCritical => true;
        public string Message => "The addon requires a newer version of itself (" + MaximalVersion + " and you have " + SkEditorAPI.Core.GetAppVersion() + ").";
    }
    class LoadingExceptionError : IAddonLoadingError
    {
        private Exception Exception { get; }
        public LoadingExceptionError(Exception exception)
        {
            Exception = exception;
        }

        public bool IsCritical => true;
        public string Message => "An exception occurred while loading the addon: " + Exception.Message;
    }
    class MissingAddonDependencyError : IAddonLoadingError
    {
        private string AddonIdentifier { get; }
        public MissingAddonDependencyError(string addonIdentifier)
        {
            AddonIdentifier = addonIdentifier;
        }

        public bool IsCritical => true;
        public string Message => "The addon requires the addon '" + AddonIdentifier + "' to be loaded.";
    }
    class OutdatedDependencyError : IAddonLoadingError
    {
        private string DependencyName { get; }
        private NuGetVersion Target { get; }
        private NuGetVersion Found { get; }
        public OutdatedDependencyError(string dependencyName, NuGetVersion target, NuGetVersion found)
        {
            DependencyName = dependencyName;
            Target = target;
            Found = found;
        }

        public bool IsCritical => true;
        public string Message => "The addon requires the dependency '" + DependencyName + "' to be at least version " + Target + ", but you have version " + Found + ".";
    }
    class FailedToLoadDependencyError : IAddonLoadingError
    {
        private string DependencyName { get; }
        private string EMessage { get; }
        public FailedToLoadDependencyError(string dependencyName, string message)
        {
            DependencyName = dependencyName;
            EMessage = message;
        }

        public bool IsCritical => true;
        public string Message => "Failed to load the dependency '" + DependencyName + "': " + EMessage;
    }
}