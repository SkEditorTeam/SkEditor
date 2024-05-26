using System;
using SkEditor.API;

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
    
}