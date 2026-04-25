using System;
using System.ComponentModel;
using Avalonia.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Utilities.Localization;
using AppSettings = SkEditor.Utilities.Settings.Configs.AppSettings;

namespace SkEditor.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppSettings _appSettings;
    private readonly ITranslationService _translation;

    [ObservableProperty]
    private string _currentFont = string.Empty;

    [ObservableProperty]
    private string _skEditorString = string.Empty;

    public SettingsViewModel(AppSettings appSettings, ITranslationService translation)
    {
        _appSettings = appSettings;
        _translation = translation;

        UpdateProperties();
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        // Use weak event handlers to avoid memory leaks
        WeakEventHandlerManager.Subscribe<AppSettings, PropertyChangedEventArgs, SettingsViewModel>(
            _appSettings,
            nameof(INotifyPropertyChanged.PropertyChanged),
            OnAppConfigPropertyChanged);

        // WeakEventHandlerManager.Subscribe<IEvents, LanguageChangedEventArgs, SettingsViewModel>(
        //     SkEditorAPI.Events,
        //     nameof(SkEditorAPI.Events.OnLanguageChanged),
        //     OnLanguageChanged);
    }

    private void OnAppConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppSettings.EditorFontFamily))
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
        CurrentFont = _translation.Get(TranslationKey.SettingsPersonalizationFontDescription)
            .Replace("{0}", _appSettings.EditorFontFamily ?? "Default (JetBrains Mono)");

        SkEditorString = $"SkEditor v{App.GetInformationalVersion()}";
    }
}