using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using SkEditor.Controls.Docs;
using SkEditor.Utilities.Docs;
using SkEditor.Utilities.Docs.Local;

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
            GroupBy groupBy = (GroupBy)GroupByComboBox.SelectedIndex;
            await LoadCategories(groupBy);
        };
        DeleteEverythingButton.Command = new AsyncRelayCommand(async () =>
        {
            await LocalProvider.Get().DeleteAll();
            await LoadCategories(GroupBy.Provider);
        });

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
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
        List<LocalDocEntry> elements = await LocalProvider.Get().GetElements();
        List<DocProvider> providers = elements.Select(x => x.OriginalProvider).Distinct().ToList();
        List<List<LocalDocEntry>> providerGroups =
            providers.Select(x => elements.FindAll(y => y.OriginalProvider == x)).ToList();

        CategoriesPanel.Children.Clear();

        foreach (List<LocalDocEntry> providerGroup in providerGroups)
        {
            DocProvider provider = providerGroup.First().OriginalProvider;
            SettingsExpander expander = CreateExpander(provider.ToString(),
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
        List<LocalDocEntry> elements = await LocalProvider.Get().GetElements();
        List<IDocumentationEntry.Type> types = elements.Select(x => x.DocType).Distinct().ToList();
        List<List<LocalDocEntry>> typeGroups = types.Select(x => elements.FindAll(y => y.DocType == x)).ToList();

        CategoriesPanel.Children.Clear();

        foreach (List<LocalDocEntry> typeGroup in typeGroups)
        {
            IDocumentationEntry.Type type = typeGroup.First().DocType;
            SettingsExpander expander = CreateExpander(type.ToString(),
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
        List<LocalDocEntry> elements = await LocalProvider.Get().GetElements();
        List<string> addons = elements.Select(x => x.Addon).Distinct().ToList();
        List<List<LocalDocEntry>> addonGroups = addons.Select(x => elements.FindAll(y => y.Addon == x)).ToList();

        CategoriesPanel.Children.Clear();

        foreach (List<LocalDocEntry> addonGroup in addonGroups)
        {
            string addon = addonGroup.First().Addon;
            SettingsExpander expander = CreateExpander(addon,
                null, addonGroup.Count);

            foreach (LocalDocEntry element in addonGroup)
            {
                DocManagementEntry entry = new(element);
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