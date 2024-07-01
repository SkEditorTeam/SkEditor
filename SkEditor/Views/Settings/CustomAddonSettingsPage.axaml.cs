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
    public static void Load(IAddon addon, 
        List<Setting>? settings = null, 
        SubSettings? parent = null)
    {
        _instance._parent = parent;
        
        _instance.LoadBasics(addon);
        _instance.PopulateSettings(settings ?? addon.GetSettings());
    }
    
    public CustomAddonSettingsPage()
    {
        InitializeComponent();
        _instance = this;
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
            Load(addon, _parent.Settings, null);
        });
    }
    
    public void PopulateSettings(List<Setting> settings)
    {
        
        ItemStackPanel.Children.Clear();
        foreach (var setting in settings)
        {
            var expander = new SettingsExpander()
            {
                Header = setting.Name,
                IconSource = setting.Icon,
                Description = setting.Description
            };

            var value = setting.Type.IsSelfManaged ? null : AddonSettingsManager.GetValue(setting);
            var control = setting.Type.CreateControl(value, 
                newValue =>
                {
                    if (setting.Type.IsSelfManaged)
                    {
                        setting.OnChanged?.Invoke(newValue);
                        return;
                    }
                    
                    (SkEditorAPI.Events as Events).AddonSettingChanged(setting, value);
                    
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