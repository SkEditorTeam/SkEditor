using System.Collections.Generic;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Serilog;
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
            if (SettingsWindow.Instance.FrameView.CanGoBack)
            {
                SettingsWindow.Instance.FrameView.GoBack();
            }
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

            object? initialValueForControl;
            object? capturedValueForLambda;

            if (setting.Type.IsSelfManaged)
            {
                initialValueForControl = setting.DefaultValue;
                capturedValueForLambda = null;
            }
            else
            {
                initialValueForControl = AddonSettingsManager.GetValue(setting);
                capturedValueForLambda = initialValueForControl;
            }

            if (initialValueForControl == null)
            {
                initialValueForControl = setting.DefaultValue;
                capturedValueForLambda = initialValueForControl;
            }

            Control control = setting.Type.CreateControl(initialValueForControl,
                newValue =>
                {
                    if (!setting.Type.IsSelfManaged)
                    {
                        if (Equals(capturedValueForLambda, newValue)) return;

                        AddonSettingsManager.SetValue(setting, newValue ?? setting.DefaultValue);

                        SkEditorAPI.Events.AddonSettingChanged(setting,
                                                               capturedValueForLambda ?? setting.DefaultValue);
                    }

                    setting.OnChanged?.Invoke(newValue ?? setting.DefaultValue);
                });

            if (control != null)
            {
                expander.Footer = control;
            }

            setting.Type.SetupExpander(expander, setting);
            ItemStackPanel.Children.Add(expander);
        }
    }

    public record SubSettings(string Name, List<Setting> Settings);
}