using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;

namespace SkEditor.Views;

public partial class FontSelectionWindow : AppWindow
{
    private static FontInfo? _selectedFont;

    public FontSelectionWindow()
    {
        InitializeComponent();
        Focusable = true;

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
                .FirstOrDefault(x => SearchBox.Text != null && x.Name != null && x.Name.StartsWith(SearchBox.Text, StringComparison.CurrentCultureIgnoreCase));
        };

        FontListBox.SelectionChanged += (_, _) => { _selectedFont = FontListBox.SelectedItem as FontInfo; };

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };
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

        FontListBox.SelectedItem = SkEditorAPI.Core.GetAppConfig().Font;
        FontListBox.ScrollIntoView(FontListBox.SelectedItem);
    }
}

public class FontInfo
{
    public string? Name { get; set; }
    public FontFamily? Font { get; set; }
}