using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using SkEditor.Controls.Docs;
using SkEditor.Utilities.Docs;
using SkEditor.Utilities.Docs.Local;
using System.Linq;

namespace SkEditor.Views;

public partial class LocalDocsManagerWindow : AppWindow
{
    public enum GroupBy
    {
        Provider,
        Type,
        Addon
    }

    public LocalDocsManagerWindow()
    {
        InitializeComponent();
        Focusable = true;

        AssignCommands();
        LoadCategories(GroupBy.Provider);
    }

    public void AssignCommands()
    {
        GroupByComboBox.SelectionChanged += (sender, args) =>
        {
            var groupBy = (GroupBy)GroupByComboBox.SelectedIndex;
            LoadCategories(groupBy);
        };
        DeleteEverythingButton.Command = new RelayCommand(async () =>
        {
            await LocalProvider.Get().DeleteAll();
            LoadCategories(GroupBy.Provider);
        });

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape) Close();
        };
    }

    public void LoadCategories(GroupBy groupBy)
    {
        GroupByComboBox.SelectedIndex = (int)groupBy;
        switch (groupBy)
        {
            case GroupBy.Provider:
                LoadByProviders();
                break;
            case GroupBy.Type:
                LoadByTypes();
                break;
            case GroupBy.Addon:
                LoadByAddons();
                break;
        }
    }

    #region Loaders

    public async void LoadByProviders()
    {
        var elements = await LocalProvider.Get().GetElements();
        var providers = elements.Select(x => x.OriginalProvider).Distinct().ToList();
        var providerGroups = providers.Select(x => elements.FindAll(y => y.OriginalProvider == x)).ToList();

        CategoriesPanel.Children.Clear();

        foreach (var providerGroup in providerGroups)
        {
            var provider = providerGroup.First().OriginalProvider;
            var expander = CreateExpander(provider.ToString(),
                IDocProvider.Providers[provider].Icon, providerGroup.Count);

            foreach (var element in providerGroup)
            {
                var entry = new DocManagementEntry(element);
                expander.Items.Add(entry);
            }

            CategoriesPanel.Children.Add(expander);
        }
    }

    public async void LoadByTypes()
    {
        var elements = await LocalProvider.Get().GetElements();
        var types = elements.Select(x => x.DocType).Distinct().ToList();
        var typeGroups = types.Select(x => elements.FindAll(y => y.DocType == x)).ToList();

        CategoriesPanel.Children.Clear();

        foreach (var typeGroup in typeGroups)
        {
            var type = typeGroup.First().DocType;
            var expander = CreateExpander(type.ToString(),
                IDocumentationEntry.GetTypeIcon(type), typeGroup.Count);

            foreach (var element in typeGroup)
            {
                var entry = new DocManagementEntry(element);
                expander.Items.Add(entry);
            }

            CategoriesPanel.Children.Add(expander);
        }
    }

    public async void LoadByAddons()
    {
        var elements = await LocalProvider.Get().GetElements();
        var addons = elements.Select(x => x.Addon).Distinct().ToList();
        var addonGroups = addons.Select(x => elements.FindAll(y => y.Addon == x)).ToList();

        CategoriesPanel.Children.Clear();

        foreach (var addonGroup in addonGroups)
        {
            var addon = addonGroup.First().Addon;
            var expander = CreateExpander(addon,
                null, addonGroup.Count);

            foreach (var element in addonGroup)
            {
                var entry = new DocManagementEntry(element);
                expander.Items.Add(entry);
            }

            CategoriesPanel.Children.Add(expander);
        }
    }

    public SettingsExpander CreateExpander(string categoryName, IconSource? icon, int elementAmount)
    {
        return new SettingsExpander()
        {
            Header = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Children =
                {
                    new TextBlock { Text = categoryName },
                    new InfoBadge { Value = elementAmount }
                }
            },
            IconSource = icon
        };
    }


    #endregion
}