using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.ViewModels;
using System.Collections.Generic;
using FluentAvalonia.UI.Controls;
using SkEditor.Controls.Addons;
using SkEditor.Utilities.InternalAPI;

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

    public void LoadAddons()
    {
        AddonsStackPanel.Children.Clear();
        foreach (var metadata in AddonLoader.Addons)
        { 
            AddonsStackPanel.Children.Add(new AddonEntryControl(metadata, this));
        }
    }

    private void AssignCommands()
    {
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(HomePage)));
    }
}
