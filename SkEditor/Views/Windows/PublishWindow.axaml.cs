using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Styling;

namespace SkEditor.Views.Windows;

public partial class PublishWindow : AppWindow
{
    public PublishWindow()
    {
        InitializeComponent();
        Focusable = true;
        InitializeUi();
    }

    public string? ApiKey { get; private set; }

    private string? CurrentService => (WebsiteComboBox.SelectedItem as ComboBoxItem)?.Content as string;

    private void InitializeUi()
    {
        WindowStyler.Style(this);
        TitleBar.ExtendsContentIntoTitleBar = false;

        PublishButton.Command = new AsyncRelayCommand(Publish);

        CopyButton.Command = new AsyncRelayCommand(async () =>
        {
            string? result = ResultTextBox.Text;
            if (result is null || Clipboard is null)
            {
                return;
            }

            await Clipboard.SetTextAsync(result);
        });

        WebsiteComboBox.SelectedIndex = SkEditorAPI.Core.GetAppConfig().LastUsedPublishService switch
        {
            "Pastebin" => 0,
            "code.skript.pl" => 1,
            "skUnity Parser" => 2,
            _ => 0
        };

        WebsiteComboBox.SelectionChanged += (_, _) => UpdateServiceInfo();

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };

        UpdateServiceInfo();
    }

    private void UpdateServiceInfo()
    {
        AppConfig appConfig = SkEditorAPI.Core.GetAppConfig();
        ApiKey = CurrentService switch
        {
            "Pastebin" => appConfig.PastebinApiKey,
            "code.skript.pl" => appConfig.CodeSkriptPlApiKey,
            "skUnity Parser" => appConfig.SkUnityApiKey,
            _ => ""
        };

        appConfig.LastUsedPublishService = CurrentService ?? string.Empty;

        AnonymouslyCheckBox.IsVisible = LanguageComboBox.IsVisible = CurrentService?.Equals("code.skript.pl") == true;
    }

    private async Task Publish()
    {
        if (!SkEditorAPI.Files.IsEditorOpen())
        {
            await SkEditorAPI.Windows.ShowError("The current opened tab is not a code editor.");
            return;
        }

        string? code = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.Text;
        if (string.IsNullOrWhiteSpace(code))
        {
            await SkEditorAPI.Windows.ShowError("You can't publish empty code!");
            return;
        }

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            await SkEditorAPI.Windows.ShowError(
                "You didn't provide an API key for this service - set it in the Connections settings.");
            return;
        }

        switch ((WebsiteComboBox.SelectedItem as ComboBoxItem)?.Content as string)
        {
            case "Pastebin":
                await CodePublisher.PublishPastebin(code, this);
                break;
            case "code.skript.pl":
                await CodePublisher.PublishCodeSkriptPl(code, this);
                break;
            case "skUnity Parser":
                await CodePublisher.PublishSkUnity(code, this);
                break;
        }
    }
}