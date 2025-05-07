using System.Collections.Generic;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.API.Settings;
using SkEditor.Utilities.InternalAPI;

namespace SkEditor.Views.Settings;

public partial class CustomAddonSettingsPage : UserControl
{
    private static CustomAddonSettingsPage _instance = null!;

    private SubSettings? _parent;

    public CustomAddonSettingsPage()
    {
        InitializeComponent();
        _instance = this;
    }

    public static void Load(IAddon addon,
        List<Setting>? settings = null,
        SubSettings? parent = null)
    {
        _instance._parent = parent;

        _instance.LoadBasics(addon);
        _instance.PopulateSettings(settings ?? addon.GetSettings());
    }

    public void LoadBasics(IAddon addon)
    {
        Title.Title = _parent == null ? addon.Name : $"{addon.Name} - {_parent.Name}";
        Title.BackButton.Command = new RelayCommand(() =>
        {
            if (_parent == null)
            {
                SettingsWindow.NavigateToPage(typeof(AddonsPage));
                return;
            }

            SettingsWindow.NavigateToPage(typeof(CustomAddonSettingsPage));
            Load(addon, _parent.Settings);
        });
    }

    public void PopulateSettings(List<Setting> settings)
    {
        ItemStackPanel.Children.Clear();
        foreach (Setting setting in settings)
        {
            SettingsExpander expander = new()
            {
                Header = setting.Name,
                IconSource = setting.Icon,
                Description = setting.Description
            };

            object? value = setting.Type.IsSelfManaged ? null : AddonSettingsManager.GetValue(setting);
            
            if (value is null)
            {
                return;
            }
            
            Control control = setting.Type.CreateControl(value,
                newValue =>
                {
                    if (newValue is null) return;
                    
                    if (setting.Type.IsSelfManaged)
                    {
                        setting.OnChanged?.Invoke(newValue);
                        return;
                    }

                    SkEditorAPI.Events.AddonSettingChanged(setting, value);

                    value = newValue;
                    AddonSettingsManager.SetValue(setting, newValue);
                    setting.OnChanged?.Invoke(newValue);
                });
            expander.Footer = control;
            setting.Type.SetupExpander(expander, setting);
            ItemStackPanel.Children.Add(expander);
        }
    }

    public record SubSettings(string Name, List<Setting> Settings);
}