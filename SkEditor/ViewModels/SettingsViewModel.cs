using System;
using System.ComponentModel;
using Avalonia.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.API;
using SkEditor.Utilities;

namespace SkEditor.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _version = string.Empty;

    [ObservableProperty] private string _currentFont = string.Empty;

    public SettingsViewModel()
    {
        UpdateProperties();
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        // Use weak event handlers to avoid memory leaks
        WeakEventHandlerManager.Subscribe<AppConfig, PropertyChangedEventArgs, SettingsViewModel>(
            SkEditorAPI.Core.GetAppConfig(),
            nameof(INotifyPropertyChanged.PropertyChanged),
            OnAppConfigPropertyChanged);

        WeakEventHandlerManager.Subscribe<IEvents, LanguageChangedEventArgs, SettingsViewModel>(
            SkEditorAPI.Events,
            nameof(SkEditorAPI.Events.OnLanguageChanged),
            OnLanguageChanged);
    }

    private void OnAppConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppConfig.Font))
        {
            UpdateProperties();
        }
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        UpdateProperties();
    }

    private void UpdateProperties()
    {
        CurrentFont = Translation.Get("SettingsPersonalizationFontDescription")
            .Replace("{0}", SkEditorAPI.Core.GetAppConfig().Font);
        
        Version = Translation.Get("SettingsAboutVersionDescription").Replace("{0}",
            $"{UpdateChecker.Major}.{UpdateChecker.Minor}.{UpdateChecker.Build}");
    }
}