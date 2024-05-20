using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities.Docs;
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
        
        FilteredAddonBox.FilterMode = AutoCompleteFilterMode.Contains;
        FilteredAddonBox.AsyncPopulator = async (text, ct) =>
        {
            var provider = IDocProvider.Providers[ViewModel.Provider!.Value];
            if (provider.HasAddons)
            {
                List<string> addons;
                try
                {
                    addons = await provider.GetAddons();
                    if (addons.Count > 10)
                        addons = addons.GetRange(0, 10);
                    addons.Add("Skript");
                }
                catch (Exception e)
                {
                    ApiVault.Get().ShowError($"An error occurred while fetching the addons.\n\n{e.Message}");
                    Log.Error(e, "An error occurred while fetching the addons.");
                    return new List<string>();
                }
                
                return addons;
            }
            
            return new List<string>();
        };
        FilteredAddonBox.SelectionChanged += (sender, args) =>
        {
            ViewModel.SearchData.FilteredAddon = FilteredAddonBox.Text;
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
                        Content = provider + " (Unavailable)",
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
        OtherInformation.Text = "";
        
        if (ViewModel.Provider == null)
        {
            ApiVault.Get().ShowError("No documentation provider selected.\n\nYou may need to connect your API keys from settings to use those.");
            OtherInformation.Text = "No provider selected.";
            return;
        }
        var provider = IDocProvider.Providers[ViewModel.Provider.Value];
        var searchData = ViewModel.SearchData;
        
        var errors = provider.CanSearch(searchData);
        if (errors.Count > 0)
        {
            ApiVault.Get().ShowError(string.Join("\n", errors));
            OtherInformation.Text = "Invalid search data.";
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
                    OtherInformation.Text = "Too many results.";
                    return;
                }
            }
            
            foreach (var element in elements)
            {
                EntriesContainer.Children.Add(new DocElementControl(element, this));
            }
            
            if (elements.Count == 0) 
                OtherInformation.Text = "No results found.";
        }
        catch (Exception exception)
        {
            ApiVault.Get().ShowError($"An error occurred while fetching the documentation.\n\n{exception.Message}");
            Log.Error(exception, "An error occurred while fetching the documentation.");
            OtherInformation.Text = "An error occurred. Try again later.";
        } finally
        {
            LoadingInformation.IsVisible = false;
        }
    }

    public void FilterByType(IDocumentationEntry.Type type)
    {
        FilteredTypesBox.SelectedIndex = (int) type;
    }
    
    public void FilterByAddon(string addon)
    {
        FilteredAddonBox.Text = addon;
    }

    public void RemoveElement(DocElementControl children)
    {
        EntriesContainer.Children.Remove(children);
    }
}