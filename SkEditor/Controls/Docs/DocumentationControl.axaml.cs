using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaEdit.Utils;
using FluentAvalonia.UI.Controls;
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

        AssignCommands();
        LoadingInformation.IsVisible = false;
    }

    public void AssignCommands()
    {
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


        var availableProviders = new List<DocProvider>();
        foreach (KeyValuePair<DocProvider, IDocProvider> providerEntry in IDocProvider.Providers)
        {
            var provider = providerEntry.Value;
            if (!provider.IsAvailable())
                continue;
            
            availableProviders.Add(providerEntry.Key);
        }
        
        if (availableProviders.Count == 0)
        {
            ApiVault.Get().ShowError("No documentation providers are available.\n\nYou may need to connect your API keys from settings to use those.");
            ProviderBox.IsEnabled = false;
            ProviderBox.Items.Clear();
            ProviderBox.Items.Add(new ComboBoxItem()
            {
                Content = "No providers available",
                Tag = null
            });
            ProviderBox.SelectedIndex = 0;
        }
        else
        {
            ProviderBox.Items.Clear();
            foreach (var provider in availableProviders)
            {
                ProviderBox.Items.Add(new ComboBoxItem()
                {
                    Content = provider.ToString(),
                    Tag = provider
                });
            }
            foreach (var provider in Enum.GetValues<DocProvider>())
            {
                if (!availableProviders.Contains(provider))
                {
                    ProviderBox.Items.Add(new ComboBoxItem()
                    {
                        Content = provider.ToString() + " (Unavailable)",
                        Tag = provider,
                        IsEnabled = false
                    });
                }
            }
            
            ProviderBox.SelectedIndex = 0;
            ViewModel.Provider = availableProviders[0];
            ProviderBox.SelectionChanged += (sender, args) =>
            {
                ViewModel.Provider = (DocProvider)((ComboBoxItem)ProviderBox.SelectedItem!).Tag!;
            };
        }
    }
    
    public DocumentationViewModel ViewModel => (DocumentationViewModel)DataContext!;

    private async void SearchButtonClick(object? sender, RoutedEventArgs e)
    {
        LoadingInformation.IsVisible = false;
        if (ViewModel.Provider == null)
        {
            ApiVault.Get().ShowError("No documentation provider selected.\n\nYou may need to connect your API keys from settings to use those.");
            return;
        }
        var provider = IDocProvider.Providers[ViewModel.Provider.Value];
        var searchData = ViewModel.SearchData;
        
        var errors = provider.CanSearch(searchData);
        if (errors.Count > 0)
        {
            ApiVault.Get().ShowError(string.Join("\n", errors));
            return;
        }
        
        try
        {
            LoadingInformation.IsVisible = true;
            EntriesContainer.Children.Clear();
            
            var elements = await provider.Search(searchData);
            if (elements.Count > 100)
            {
                var result = await ApiVault.Get().ShowAdvancedMessage("Too Many Results",
                    "The search returned more than 100 results. Are you sure you want to display all of them?\n\nIt may slow down SkEditor!");
                if (result != ContentDialogResult.Primary)
                {
                    LoadingInformation.IsVisible = false;
                    return;
                }
            }
            
            foreach (var element in elements)
            {
                EntriesContainer.Children.Add(new DocElementControl(element));
            }
            
            LoadingInformation.IsVisible = false;
        }
        catch (Exception exception)
        {
            ApiVault.Get().ShowError($"An error occurred while fetching the documentation.\n\n{exception.Message}");
            Serilog.Log.Error(exception, "An error occurred while fetching the documentation.");
        } finally
        {
            LoadingInformation.IsVisible = false;
        }
    }
}