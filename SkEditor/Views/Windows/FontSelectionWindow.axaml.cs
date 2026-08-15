using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Views.Controls;

namespace SkEditor.Views.Windows;

// TODO: rework the entire font selection window
public partial class FontSelectionWindow : SkEditorWindow
{
    private static FontInfo? _selectedFont;

    public FontSelectionWindow()
    {
        InitializeComponent();

        SetUp();
        LoadFonts();
    }

    private void SetUp()
    {
        SelectButton.Command = new RelayCommand(() =>
        {
            string? fontName = _selectedFont?.Name;
            Close(fontName);
        });

        SearchBox.TextChanged += (_, _) =>
        {
            FontListBox.SelectedItem = FontListBox.Items
                .Cast<FontInfo>()
                .FirstOrDefault(x =>
                    SearchBox.Text != null && x.Name != null &&
                    x.Name.StartsWith(SearchBox.Text, StringComparison.CurrentCultureIgnoreCase));
        };

        FontListBox.SelectionChanged += (_, _) => { _selectedFont = FontListBox.SelectedItem as FontInfo; };
    }

    private void LoadFonts()
    {
        List<FontInfo> fonts = FontManager.Current.SystemFonts
            .Select(fontInfo => new FontInfo { Name = fontInfo.Name, Font = fontInfo })
            .ToList();

        object? font = null;
        Application.Current?.TryGetResource("JetBrainsFont", ThemeVariant.Default, out font);
        fonts.Insert(0, new FontInfo { Name = "Default", Font = (FontFamily?)font });

        FontListBox.ItemsSource = fonts;

        // FontListBox.SelectedItem = SkEditorAPI.Configs.GetAppSettings().EditorFontFamily ?? "Default";
        // FontListBox.ScrollIntoView(FontListBox.SelectedItem);
    }
}

public class FontInfo
{
    public FontFamily? Font { get; set; }
    public string? Name { get; set; }
}