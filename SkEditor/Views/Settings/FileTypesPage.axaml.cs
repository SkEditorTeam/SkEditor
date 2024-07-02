using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using System.Linq;

namespace SkEditor.Views.Settings;

public partial class FileTypesPage : UserControl
{
    public FileTypesPage()
    {
        InitializeComponent();
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(HomePage)));

        LoadTypes();
    }

    public void LoadTypes()
    {
        TypeContainer.Children.Clear();

        foreach (var keyValue in SkEditorAPI.Core.GetAppConfig().FileTypeChoices)
        {
            var ext = keyValue.Key;
            var fullId = keyValue.Value;

            var availableTypes = Registries.FileTypes.GetValues().ToList()
                .Where(x => x.SupportedExtensions.Contains(ext)).ToList();

            var expander = new SettingsExpander()
            {
                Header = ext
            };

            var box = new ComboBox()
            {
                MinWidth = 300,
                HorizontalContentAlignment = HorizontalAlignment.Left
            };

            int i = 0;
            int selectedIndex = -1;
            foreach (var type in availableTypes)
            {
                var grid = new Grid()
                {
                    RowDefinitions = new RowDefinitions("*,*"),
                    ColumnDefinitions = new ColumnDefinitions("Auto, *"),
                    /*RowSpacing = 10,
                    ColumnSpacing = 10*/
                };

                var title = new TextBlock
                {
                    Text = type.AddonName,
                    FontWeight = FontWeight.SemiBold,
                    FontSize = 14
                };
                grid.Children.Add(title);
                Grid.SetColumn(title, 1);
                Grid.SetRow(title, 0);

                var desc = new TextBlock
                {
                    Text = type.DisplayedDescription,
                    FontStyle = FontStyle.Italic,
                    FontSize = 10
                };
                grid.Children.Add(desc);
                Grid.SetColumn(desc, 1);
                Grid.SetRow(desc, 1);

                var icon = new IconSourceElement
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

                var item = new ComboBoxItem
                {
                    Content = grid,
                    Tag = type
                };

                if (Registries.FileTypes.GetValueKey(type).FullKey == fullId)
                {
                    selectedIndex = i;
                }

                box.Items.Add(item);
                i++;
            }

            box.SelectedIndex = selectedIndex;

            box.SelectionChanged += (sender, args) =>
            {
                if (box.SelectedItem is ComboBoxItem item)
                {
                    var type = (FileTypeData)item.Tag;
                    SkEditorAPI.Core.GetAppConfig().FileTypeChoices[ext] = Registries.FileTypes.GetValueKey(type).FullKey;
                }
            };

            var removeBtn = new Button
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

            expander.Footer = new StackPanel()
            {
                Children = { box, removeBtn },
                Orientation = Orientation.Horizontal,
                Spacing = 5
            };
            TypeContainer.Children.Add(expander);
        }
    }
}