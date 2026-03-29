using System.Threading.Tasks;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.Utilities.Styling;
using SkEditor.ViewModels.Windows;

namespace SkEditor.Views.Windows;

public partial class PublishWindow : AppWindow
{
    public PublishWindow()
    {
        InitializeComponent();
        Focusable = true;
        InitializeUi();
    }

    public PublishWindow(PublishWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

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

        // WebsiteComboBox.SelectedIndex = SkEditorAPI.Configs.GetStateConfig().LastUsedPublishService switch
        // {
        //     "Pastebin" => 0,
        //     "code.skript.pl" => 1,
        //     "skUnity Parser" => 2,
        //     _ => 0
        // };

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };
    }

    private async Task Publish()
    {
        // if (!SkEditorAPI.Files.IsEditorOpen())
        // {
        //     await SkEditorAPI.Windows.ShowError("The current opened tab is not a code editor.");
        //     return;
        // }
        //
        // string? code = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.Text;
        // if (string.IsNullOrWhiteSpace(code))
        // {
        //     await SkEditorAPI.Windows.ShowError("You can't publish empty code!");
        //     return;
        // }
        //
        // if (string.IsNullOrWhiteSpace(ApiKey))
        // {
        //     await SkEditorAPI.Windows.ShowError(
        //         "You didn't provide an API key for this service - set it in the Connections settings.");
        //     return;
        // }
        //
        // switch ((WebsiteComboBox.SelectedItem as ComboBoxItem)?.Content as string)
        // {
        //     case "Pastebin":
        //         await CodePublisher.PublishPastebin(code, this);
        //         break;
        //     case "code.skript.pl":
        //         await CodePublisher.PublishCodeSkriptPl(code, this);
        //         break;
        //     case "skUnity Parser":
        //         await CodePublisher.PublishSkUnity(code, this);
        //         break;
        // }
    }
}