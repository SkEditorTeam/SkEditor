using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Docs;
using SkEditor.Utilities.Docs.SkriptHub;
using SkEditor.ViewModels;
using SkEditor.Views;

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
    }

    public void AssignCommands()
    {
        OpenLocalManagementButton.Command = new RelayCommand(async () => await new LocalDocsManagerWindow().ShowDialog(SkEditorAPI.Windows.GetMainWindow()));
        RefreshProviderButton.Command = new RelayCommand(() =>
        {
            ProviderBox.SelectionChanged -= HandleProviderBoxSelection;
            InitializeProviderBox();
        });
        DownloadAllButton.Command = new RelayCommand(DownloadAllEntries);
        RetrieveSkriptHubDocsButton.Command = new RelayCommand(async () =>
        {
            var response = await SkEditorAPI.Windows.ShowDialog(Translation.Get("Attention"),
                Translation.Get("DocumentationWindowFetchWarning"),
                new SymbolIconSource { Symbol = Symbol.ImportantFilled });
            if (response == ContentDialogResult.Primary)
            {
                var provider = IDocProvider.Providers[DocProvider.SkriptHub] as SkriptHubProvider;
                provider.DeleteEverything();
                await SkEditorAPI.Windows.ShowMessage(Translation.Get("Success"),
                    Translation.Get("DocumentationWindowFetchSuccessMessage"));
            }
        });
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
                Content = CreateEntryWithIcon(IDocumentationEntry.GetTypeIcon(type), type.ToString()),
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
        FilteredAddonBox.FilterMode = AutoCompleteFilterMode.None;
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
                await SkEditorAPI.Windows.ShowError($"An error occurred while fetching the addons.\n\n{e.Message}");
                Log.Error(e, "An error occurred while fetching the addons.");
                addons = [];
            }

            return addons;
        };
        FilteredAddonBox.TextChanged += (sender, args) =>
        {
            ViewModel.SearchData.FilteredAddon = FilteredAddonBox.Text;
        };
    }

    private void InitializeProviderBox()
    {
        var availableProviders = GetAvailableProviders();

        if (availableProviders.Count == 0)
        {
            SkEditorAPI.Windows.ShowError("No documentation providers are available.\n\nYou may need to connect your API keys from settings to use those.");
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

    private StackPanel CreateEntryWithIcon(IconSource icon, string content, FontWeight fontWeight = FontWeight.Normal)
    {
        return new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Spacing = 6,
            Children =
            {
                new IconSourceElement()
                {
                    IconSource = icon,
                    Width = 16,
                    Height = 16
                },
                new TextBlock()
                {
                    Text = content,
                    FontWeight = fontWeight
                }
            }
        };
    }

    private void PopulateProviderBox(List<DocProvider> availableProviders)
    {
        StackPanel CreatePanel(DocProvider provider, string content)
        {
            return CreateEntryWithIcon(IDocProvider.Providers[provider].Icon, content,
                provider == DocProvider.SkriptHub ? FontWeight.SemiBold : FontWeight.Normal);
        }

        ProviderBox.Items.Clear();
        foreach (var provider in availableProviders)
        {
            var comboBox = new ComboBoxItem()
            {
                Content = CreatePanel(provider, provider.ToString()),
                Tag = provider
            };

            ProviderBox.Items.Add(comboBox);
        }
        foreach (var provider in Enum.GetValues<DocProvider>())
        {
            if (!availableProviders.Contains(provider))
            {
                ProviderBox.Items.Add(new ComboBoxItem()
                {
                    Content = CreatePanel(provider, provider.ToString() + " (Unavailable)"),
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
        RetrieveSkriptHubDocsButton.IsVisible = ViewModel.Provider == DocProvider.SkriptHub;
    }

    public DocumentationViewModel ViewModel => (DocumentationViewModel)DataContext!;

    private async void SearchButtonClick(object? sender, RoutedEventArgs e)
    {
        LoadingInformation.IsVisible = false;
        OtherInformation.Text = "";
        EntriesContainer.Children.Clear();

        if (!ValidateProvider(out var provider)) return;

        var searchData = ViewModel.SearchData;
        if (!ValidateSearchData(provider, searchData)) return;

        await PerformSearch(provider, searchData);
    }

    private bool ValidateProvider(out IDocProvider? provider)
    {
        provider = null;
        if (ViewModel.Provider == null)
        {
            SkEditorAPI.Windows.ShowError(Translation.Get("DocumentationWindowNoProvidersMessage"));
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
            SkEditorAPI.Windows.ShowError(string.Join("\n", errors));
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
            await SkEditorAPI.Windows.ShowError(Translation.Get("DocumentationWindowNoEntries"));
            return;
        }

        if (EntriesContainer.Children.Count > 0)
        {
            var result = await SkEditorAPI.Windows.ShowDialog("Download all",
                Translation.Get("DocumentationWindowDownloadAllMessage", EntriesContainer.Children.Count.ToString()),
                primaryButtonText: "Yes", cancelButtonText: "No");

            if (result != ContentDialogResult.Primary) return;
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

        taskDialog.XamlRoot = SkEditorAPI.Windows.GetMainWindow();
        await taskDialog.ShowAsync();
    }

    private static async Task<bool> ConfirmLargeResults()
    {
        var result = await SkEditorAPI.Windows.ShowDialog(Translation.Get("DocumentationWindowTooManyResults"),
            Translation.Get("DocumentationWindowTooManyResultsMessage"), primaryButtonText: "Yes", cancelButtonText: "No");
        return result == ContentDialogResult.Primary;
    }

    private void HandleSearchResults(List<IDocumentationEntry> elements)
    {
        foreach (var element in elements)
        {
            Task.Run(async () => await Dispatcher.UIThread.InvokeAsync(() =>
            {
                EntriesContainer.Children.Add(new DocElementControl(element, this));
            }));
        }

        if (elements.Count == 0)
        {
            OtherInformation.Text = Translation.Get("DocumentationWindowNoResults");
        }
    }

    private void HandleError(Exception exception)
    {
        SkEditorAPI.Windows.ShowError(Translation.Get("DocumentationWindowErrorGlobal", exception.Message));
        Log.Error(exception, "An error occurred while fetching the documentation.");
        OtherInformation.Text = Translation.Get("DocumentationWindowAnErrorOccured");
    }

    public void FilterByType(IDocumentationEntry.Type type) => FilteredTypesBox.SelectedIndex = (int)type;

    public void FilterByAddon(string addon) => FilteredAddonBox.Text = addon;

    public void RemoveElement(DocElementControl children) => EntriesContainer.Children.Remove(children);
}