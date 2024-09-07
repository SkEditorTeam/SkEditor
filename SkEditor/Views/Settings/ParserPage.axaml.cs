using System;
using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Interactivity;
using SkEditor.ViewModels;

namespace SkEditor.Views.Settings;
public partial class ParserPage : UserControl
{
    
    public ParserPage()
    {
        InitializeComponent();

        try
        {
            var config = SkEditorAPI.Core.GetAppConfig();
            SkEditorAPI.Logs.Debug("Loading parser settings: " + string.Join(", ", config.IgnoredParserWarnings));
            DataContext = new ParserSettingsViewModel
            {
                Warnings = Registries.ParserWarnings.Select(warning => new ParserSettingsViewModel.WarningData(
                    warning,
                    config.IgnoredParserWarnings.TryGetValue(warning.Identifier, out var isEnabled) && isEnabled
                )).ToList()
            };
            
            Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(HomePage)));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
