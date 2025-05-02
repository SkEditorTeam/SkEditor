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
using System.Threading.Tasks;

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
        _ = LoadCategories(GroupBy.Provider);
    }

    public void AssignCommands()
    {
        GroupByComboBox.SelectionChanged += async (_, _) =>
        {
            var groupBy = (GroupBy)GroupByComboBox.SelectedIndex;
            await LoadCategories(groupBy);
        };
        DeleteEverythingButton.Command = new AsyncRelayCommand(async () =>
        {
            await LocalProvider.Get().DeleteAll();
            await LoadCategories(GroupBy.Provider);
        });

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape) Close();
        };
    }

    public async Task LoadCategories(GroupBy groupBy)
    {
        GroupByComboBox.SelectedIndex = (int)groupBy;
        switch (groupBy)
        {
            case GroupBy.Provider:
                await LoadByProviders();
                break;
            case GroupBy.Type:
                await LoadByTypes();
                break;
            case GroupBy.Addon:
                await LoadByAddons();
                break;
        }
    }

    #region Loaders

    public async Task LoadByProviders()
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

            foreach (DocManagementEntry entry in providerGroup.Select(element => new DocManagementEntry(element)))
            {
                expander.Items.Add(entry);
            }

            CategoriesPanel.Children.Add(expander);
        }
    }

    public async Task LoadByTypes()
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

            foreach (DocManagementEntry entry in typeGroup.Select(element => new DocManagementEntry(element)))
            {
                expander.Items.Add(entry);
            }

            CategoriesPanel.Children.Add(expander);
        }
    }

    public async Task LoadByAddons()
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

    public static SettingsExpander CreateExpander(string categoryName, IconSource? icon, int elementAmount)
    {
        return new SettingsExpander
        {
            Header = new StackPanel
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