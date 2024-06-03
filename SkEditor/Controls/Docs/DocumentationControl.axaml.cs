using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Docs;
using SkEditor.ViewModels;
using SkEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkEditor.Controls.Docs;

public partial class DocumentationControl : UserControl
{
    public DocumentationControl()
    {
        InitializeComponent();
        DataContext = new DocumentationViewModel();

        AssignCommands();
        AddItems();
        LoadingInformation.IsVisible = false;
        SearchQueryInput.Loaded += (sender, e) => SearchQueryInput.Focus();

        KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Enter) SearchButtonClick(sender, e);
        };

    }

    public void AssignCommands()
    {
        OpenLocalManagementButton.Command = new RelayCommand(async () => await new LocalDocsManagerWindow().ShowDialog(ApiVault.Get().GetMainWindow()));
        RefreshProviderButton.Command = new RelayCommand(() =>
        {
            ProviderBox.SelectionChanged -= HandleProviderBoxSelection;
            InitializeProviderBox();
        });
        DownloadAllButton.Command = new RelayCommand(DownloadAllEntries);
    }

    public void AddItems()
    {
        InitializeFilteredTypesBox();
        InitializeFilteredAddonBox();
        InitializeProviderBox();
    }

    private void InitializeFilteredTypesBox()
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
    }

    private void InitializeFilteredAddonBox()
    {
        FilteredAddonBox.FilterMode = AutoCompleteFilterMode.Contains;
        FilteredAddonBox.AsyncPopulator = async (text, ct) =>
        {
            var provider = IDocProvider.Providers[ViewModel.Provider!.Value];
            if (!provider.HasAddons) return new List<string>();

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
                addons = [];
            }

            return addons;
        };
        FilteredAddonBox.SelectionChanged += (sender, args) =>
        {
            ViewModel.SearchData.FilteredAddon = FilteredAddonBox.Text;
        };
    }

    private void InitializeProviderBox()
    {
        var availableProviders = GetAvailableProviders();

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
            return;
        }

        PopulateProviderBox(availableProviders);
    }

    private static List<DocProvider> GetAvailableProviders()
    {
        return IDocProvider.Providers.Where(pair => pair.Value.IsAvailable()).Select(pair => pair.Key).ToList();
    }

    private void PopulateProviderBox(List<DocProvider> availableProviders)
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
        ProviderBox.SelectionChanged += HandleProviderBoxSelection;
    }

    public void HandleProviderBoxSelection(object? sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
        ViewModel.Provider = (DocProvider)((ComboBoxItem)ProviderBox.SelectedItem!).Tag!;

        OpenLocalManagementButton.IsVisible = ViewModel.Provider == DocProvider.Local;
        DownloadAllButton.IsVisible = ViewModel.Provider != DocProvider.Local;
    }

    public DocumentationViewModel ViewModel => (DocumentationViewModel)DataContext!;

    private async void SearchButtonClick(object? sender, RoutedEventArgs e)
    {
        LoadingInformation.IsVisible = false;
        OtherInformation.Text = "";

        if (!ValidateProvider(out var provider)) return;

        var searchData = ViewModel.SearchData;
        if (!ValidateSearchData(provider, searchData)) return;

        await PerformSearch(provider, searchData);
    }

    private bool ValidateProvider(out IDocProvider provider)
    {
        provider = null;
        if (ViewModel.Provider == null)
        {
            ApiVault.Get().ShowError(Translation.Get("DocumentationWindowNoProvidersMessage"));
            OtherInformation.Text = Translation.Get("DocumentationWindowNoProviders");
            return false;
        }
        provider = IDocProvider.Providers[ViewModel.Provider.Value];
        return true;
    }

    private bool ValidateSearchData(IDocProvider provider, SearchData searchData)
    {
        var errors = provider.CanSearch(searchData);
        if (errors.Count > 0)
        {
            ApiVault.Get().ShowError(string.Join("\n", errors));
            OtherInformation.Text = Translation.Get("DocumentationWindowInvalidData");
            return false;
        }
        return true;
    }

    private async Task PerformSearch(IDocProvider provider, SearchData searchData)
    {
        try
        {
            LoadingInformation.IsVisible = true;
            EntriesContainer.Children.Clear();

            var elements = await provider.Search(searchData);
            if (elements.Count > 100 && !await ConfirmLargeResults())
            {
                LoadingInformation.IsVisible = false;
                OtherInformation.Text = Translation.Get("DocumentationWindowTooManyResults");
                return;
            }

            HandleSearchResults(elements);
        }
        catch (Exception exception)
        {
            HandleError(exception);
        }
        finally
        {
            LoadingInformation.IsVisible = false;
        }
    }

    private async void DownloadAllEntries()
    {
        if (EntriesContainer.Children.Count == 0)
        {
            ApiVault.Get().ShowError(Translation.Get("DocumentationWindowNoEntries"));
            return;
        }

        if (EntriesContainer.Children.Count > 0)
        {
            var result = await ApiVault.Get().ShowAdvancedMessage("Download all",
                Translation.Get("DocumentationWindowDownloadAllMessage", EntriesContainer.Children.Count.ToString()));
            if (result != ContentDialogResult.Primary)
                return;
        }

        var taskDialog = new TaskDialog
        {
            Title = Translation.Get("DocumentationWindowDownloadingElements"),
            ShowProgressBar = true,
            IconSource = new SymbolIconSource { Symbol = Symbol.Download },
            SubHeader = Translation.Get("DocumentationWindowDownloading"),
            Content = Translation.Get("DocumentationWindowDownloadingMessage")
        };
        var chidren = EntriesContainer.Children;
        taskDialog.SetProgressBarState(0, TaskDialogProgressState.Normal);

        taskDialog.Opened += async (sender, args) =>
        {
            for (var index = 0; index < EntriesContainer.Children.Count; index++)
            {
                var element = EntriesContainer.Children[index];
                if (element is DocElementControl docElement)
                    await docElement.ForceDownloadElement();
                taskDialog.SetProgressBarState((index + 1) * 100 / EntriesContainer.Children.Count,
                    TaskDialogProgressState.Normal);
            }

            taskDialog.Hide(TaskDialogStandardResult.OK);
        };

        taskDialog.XamlRoot = ApiVault.Get().GetMainWindow();
        await taskDialog.ShowAsync();
    }

    private static async Task<bool> ConfirmLargeResults()
    {
        var result = await ApiVault.Get().ShowAdvancedMessage(Translation.Get("DocumentationWindowTooManyResults"),
            Translation.Get("DocumentationWindowTooManyResultsMessage"));
        return result == ContentDialogResult.Primary;
    }

    private void HandleSearchResults(List<IDocumentationEntry> elements)
    {
        foreach (var element in elements)
        {
            EntriesContainer.Children.Add(new DocElementControl(element, this));
        }

        if (elements.Count == 0)
        {
            OtherInformation.Text = Translation.Get("DocumentationWindowNoResults");
        }
    }

    private void HandleError(Exception exception)
    {
        ApiVault.Get().ShowError(Translation.Get("DocumentationWindowErrorGlobal", exception.Message));
        Log.Error(exception, "An error occurred while fetching the documentation.");
        OtherInformation.Text = Translation.Get("DocumentationWindowAnErrorOccured");
    }

    public void FilterByType(IDocumentationEntry.Type type) => FilteredTypesBox.SelectedIndex = (int)type;

    public void FilterByAddon(string addon) => FilteredAddonBox.Text = addon;

    public void RemoveElement(DocElementControl children) => EntriesContainer.Children.Remove(children);
}