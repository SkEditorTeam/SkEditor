using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Controls;
using SkEditor.Utilities.Styling;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SkEditor.Views.Settings;
public partial class ThemePage : UserControl
{
    public static Dictionary<ColorPickerSettingsExpander, string> ColorMappings = new();

    public ThemePage()
    {
        InitializeComponent();

        AssignCommands();
        LoadThemes();
    }

    private void LoadThemes()
    {
        IEnumerable<ComboBoxItem> themeItems = ThemeEditor.Themes.Select(theme => new ComboBoxItem
        {
            Content = theme.Name,
            Tag = theme.FileName
        });
        
        foreach (ComboBoxItem item in themeItems)
        {
            ThemeComboBox.Items.Add(item);
        }

        ThemeComboBox.SelectedItem = ThemeComboBox.Items.FirstOrDefault(x => ((ComboBoxItem)x).Tag.Equals(ThemeEditor.CurrentTheme.FileName));

        ThemeComboBox.SelectionChanged += (_, _) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                ComboBoxItem item = (ComboBoxItem)ThemeComboBox.SelectedItem;
                Theme theme = ThemeEditor.Themes.FirstOrDefault(x => x.FileName.Equals(item.Tag));
                _ = ThemeEditor.SetTheme(theme);
                SettingsWindow.Instance.Theme = ThemeEditor.SmallWindowTheme;
            });
        };
    }


    private void AssignCommands()
    {
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(PersonalizationPage)));

        OpenThemesFolderButton.Command = new RelayCommand(() => Process.Start(new ProcessStartInfo()
        {
            FileName = ThemeEditor.ThemeFolderPath,
            UseShellExecute = true
        }));

        EditThemeItem.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(Personalization.EditThemePage)));
    }
}
