using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Files;
using SkEditor.Views.Settings.Personalization;

namespace SkEditor.Views.Settings;

public partial class PersonalizationPage : UserControl
{
    public PersonalizationPage()
    {
        InitializeComponent();

        DataContext = SkEditorAPI.Core.GetAppConfig();

        AssignCommands();
    }

    private void AssignCommands()
    {
        ThemePageButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(ThemePage)));
        SyntaxPageButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(FileSyntaxes)));
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(HomePage)));

        FontButton.Command = new AsyncRelayCommand(SelectFont);

        HighlightCurrentLineSwitch.Command = new RelayCommand(() =>
        {
            foreach (TextEditor textEditor in SkEditorAPI.Files.GetOpenedEditors().Select(x => x.Editor)!)
            {
                textEditor!.Options.HighlightCurrentLine = !textEditor.Options.HighlightCurrentLine;
            }
        });
    }

    private async Task SelectFont()
    {
        FontSelectionWindow window = new();
        string result = await window.ShowDialogOnMainWindow<string>();
        if (result is null)
        {
            return;
        }

        SkEditorAPI.Core.GetAppConfig().Font = result;
        CurrentFont.Description = Translation.Get("SettingsPersonalizationFontDescription").Replace("{0}", result);

        SkEditorAPI.Files.GetOpenedEditors().ForEach(i =>
        {
            if (!result.Equals("Default"))
            {
                i.Editor!.FontFamily = new FontFamily(result);
                return;
            }

            object? jetbrainsMonoFont = FileBuilder.GetJetbrainsMonoFont();
            if (jetbrainsMonoFont is null)
            {
                i.Editor!.FontFamily = new FontFamily("JetBrains Mono");
            }
            else
            {
                i.Editor!.FontFamily = (FontFamily)jetbrainsMonoFont;
            }
        });
    }
}