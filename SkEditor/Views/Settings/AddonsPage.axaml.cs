using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.ViewModels;
using System.Collections.Generic;

namespace SkEditor.Views.Settings;
public partial class AddonsPage : UserControl
{
    public AddonsPage()
    {
        InitializeComponent();

        LoadAddons();
        AssignCommands();

        DataContext = new SettingsViewModel();
    }

    private void LoadAddons()
    {
        List<IAddon> addons = AddonLoader.Addons;
        addons.ForEach(addon =>
        {
            ListBoxItem item = new()
            {
                Content = addon.Name,
                Tag = addon
            };
            AddonListBox.Items.Add(item);
        });
    }

    private void AssignCommands()
    {
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(HomePage)));
    }
}
