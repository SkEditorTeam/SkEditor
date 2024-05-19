using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaEdit.Utils;
using SkEditor.API;
using SkEditor.Utilities.Docs;
using SkEditor.Utilities.Docs.SkriptHub;
using SkEditor.Utilities.Docs.SkUnity;
using SkEditor.ViewModels;

namespace SkEditor.Controls.Docs;

public partial class DocumentationControl : UserControl
{
    public DocumentationControl()
    {
        InitializeComponent();
        DataContext = new DocumentationViewModel();
        
        foreach (IDocumentationEntry.Type type in IDocumentationEntry.AllTypes)
        {
            FilteredTypesBox.Items.Add(new ComboBoxItem()
            {
                Content = type.ToString(),
                Tag = type
            });
        }
        FilteredTypesBox.SelectedIndex = 0;
        FilteredTypesBox.SelectionChanged += (sender, args) =>
        {
            ViewModel.SearchData.FilteredType = (IDocumentationEntry.Type)((ComboBoxItem)FilteredTypesBox.SelectedItem!).Tag!;
        };
    }
    
    public DocumentationViewModel ViewModel => (DocumentationViewModel)DataContext!;

    private async void SearchButtonClick(object? sender, RoutedEventArgs e)
    {
        var provider = SkriptHubProvider.Get();
        var searchData = ViewModel.SearchData;
        
        var errors = provider.CanSearch(searchData);
        if (errors.Count > 0)
        {
            ApiVault.Get().ShowError(string.Join("\n", errors));
            return;
        }
        
        try
        {
            var elements = await provider.Search(searchData);

            EntriesContainer.Children.Clear();
            foreach (var element in elements)
            {
                EntriesContainer.Children.Add(new DocElementControl(element));
            }
        }
        catch (Exception exception)
        {
            ApiVault.Get().ShowError($"An error occurred while fetching the documentation.\n\n{exception.Message}");
        }
    }
}