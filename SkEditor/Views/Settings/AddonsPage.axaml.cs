using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.ViewModels;
using System.Collections.Generic;
using FluentAvalonia.UI.Controls;
using SkEditor.Controls.Addons;

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
        List<IAddon> addons = AddonLoader.AllAddons;
        addons.ForEach(addon =>
        {
            AddonsStackPanel.Children.Add(new AddonEntryControl(addon));
        });
    }

    private void AssignCommands()
    {
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(HomePage)));
    }
}
