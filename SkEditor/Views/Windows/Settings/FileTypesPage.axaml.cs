using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;

namespace SkEditor.Views.Windows.Settings;

public partial class FileTypesPage : UserControl
{
    public FileTypesPage()
    {
        InitializeComponent();

        LoadTypes();
    }

    public void LoadTypes()
    {
        TypeContainer.Children.Clear();

        foreach ((string ext, string? fullId) in SkEditorAPI.Core.GetAppConfig().FileTypeChoices)
        {
            List<FileTypeData> availableTypes = Registries.FileTypes.GetValues().ToList()
                .Where(x => x.SupportedExtensions.Contains(ext)).ToList();

            SettingsExpander expander = new()
            {
                Header = ext
            };

            ComboBox box = new()
            {
                MinWidth = 300,
                HorizontalContentAlignment = HorizontalAlignment.Left
            };

            int i = 0;
            int selectedIndex = -1;
            foreach (FileTypeData type in availableTypes)
            {
                Grid grid = new()
                {
                    RowDefinitions = new RowDefinitions("*,*"),
                    ColumnDefinitions = new ColumnDefinitions("Auto, *")
                    /*RowSpacing = 10,
                    ColumnSpacing = 10*/
                };

                TextBlock title = new()
                {
                    Text = type.AddonName,
                    FontWeight = FontWeight.SemiBold,
                    FontSize = 14
                };
                grid.Children.Add(title);
                Grid.SetColumn(title, 1);
                Grid.SetRow(title, 0);

                TextBlock desc = new()
                {
                    Text = type.DisplayedDescription,
                    FontStyle = FontStyle.Italic,
                    FontSize = 10
                };
                grid.Children.Add(desc);
                Grid.SetColumn(desc, 1);
                Grid.SetRow(desc, 1);

                IconSourceElement icon = new()
                {
                    IconSource = type.Icon,
                    Width = 32,
                    Height = 32,
                    Margin = new Thickness(0, 0, 5, 0)
                };
                grid.Children.Add(icon);
                Grid.SetColumn(icon, 0);
                Grid.SetRow(icon, 0);
                Grid.SetRowSpan(icon, 2);

                ComboBoxItem item = new()
                {
                    Content = grid,
                    Tag = type
                };

                if (Registries.FileTypes.GetValueKey(type)?.FullKey == fullId)
                {
                    selectedIndex = i;
                }

                box.Items.Add(item);
                i++;
            }

            box.SelectedIndex = selectedIndex;

            box.SelectionChanged += (_, _) =>
            {
                if (box.SelectedItem is not ComboBoxItem item)
                {
                    return;
                }

                FileTypeData? type = (FileTypeData?)item.Tag;
                if (type is null) return;
                SkEditorAPI.Core.GetAppConfig().FileTypeChoices[ext] = Registries.FileTypes.GetValueKey(type)?.FullKey;
            };

            Button removeBtn = new()
            {
                Content = "Remove",
                Command = new RelayCommand(() =>
                {
                    SkEditorAPI.Core.GetAppConfig().FileTypeChoices.Remove(ext);
                    LoadTypes();
                }),
                VerticalContentAlignment = VerticalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            expander.Footer = new StackPanel
            {
                Children = { box, removeBtn },
                Orientation = Orientation.Horizontal,
                Spacing = 5
            };
            TypeContainer.Children.Add(expander);
        }
    }
}