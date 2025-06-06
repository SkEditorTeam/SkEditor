using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.Controls.Addons;
using SkEditor.Utilities;
using SkEditor.Utilities.InternalAPI;
using SkEditor.ViewModels;

namespace SkEditor.Views.Settings;

public partial class AddonsPage : UserControl
{
    public AddonsPage()
    {
        InitializeComponent();

        LoadAddons();
        AssignCommands();

        DataContext = new SettingsViewModel();
    }

    public void LoadAddons()
    {
        AddonsStackPanel.Children.Clear();
        foreach (AddonMeta metadata in AddonLoader.Addons)
        {
            AddonsStackPanel.Children.Add(new AddonEntryControl(metadata, this));
        }
    }

    private void AssignCommands()
    {
        LoadFromFileButton.IsVisible = SkEditorAPI.Core.IsDeveloperMode();
        LoadFromFileButton.Command = new AsyncRelayCommand(async () =>
        {
            string? file = await SkEditorAPI.Windows.AskForFile(new FilePickerOpenOptions
            {
                Title = "Load Addon",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("SkEditor Addon") { Patterns = ["*.dll"] }
                ]
            });
            if (file is null)
            {
                return;
            }

            string name = Path.GetFileNameWithoutExtension(file);
            if (AddonLoader.Addons.Any(addon => addon.Addon.Identifier == name))
            {
                await SkEditorAPI.Windows.ShowMessage("Addon already loaded",
                    "This addon is already loaded. Delete it first if you want to reload it.");
                return;
            }

            string folder = Path.Combine(AppConfig.AppDataFolderPath, "Addons", name);
            string addonFile = Path.Combine(folder, Path.GetFileName(file));
            if (File.Exists(addonFile))
            {
                await SkEditorAPI.Windows.ShowMessage("Addon already loaded",
                    "This addon is already loaded. Delete it first if you want to reload it.");
                return;
            }

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.Copy(file, Path.Combine(folder, Path.GetFileName(file)));

            await AddonLoader.LoadAddonFromFile(folder);
            LoadAddons();
        });

        OpenMarketplaceButton.Command = new AsyncRelayCommand(async () =>
        {
            SettingsWindow.Instance.Close();
            await SkEditorAPI.Windows.ShowWindowAsDialog(new MarketplaceWindow());
        });
    }
}