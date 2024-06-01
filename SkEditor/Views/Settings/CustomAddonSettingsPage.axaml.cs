using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.InternalAPI;

namespace SkEditor.Views.Settings;

public partial class CustomAddonSettingsPage : UserControl
{
    private static CustomAddonSettingsPage _instance = null!;

    public static void Load(IAddon addon)
    {
        _instance.LoadBasics(addon);
        _instance.PopulateSettings(addon);
    }
    
    public CustomAddonSettingsPage()
    {
        InitializeComponent();
        _instance = this;
    }

    public void LoadBasics(IAddon addon)
    {
        Title.Title = addon.Name;
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(AddonsPage)));
    }
    
    public void PopulateSettings(IAddon addon)
    {
        
        ItemStackPanel.Children.Clear();
        foreach (var setting in addon.GetSettings())
        {
            var expander = new SettingsExpander()
            {
                Header = setting.Name,
                IconSource = setting.Icon,
                Description = setting.Description
            };

            var value = AddonSettingsManager.GetValue(setting);
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
            
            ItemStackPanel.Children.Add(expander);
        }
        
    }
}