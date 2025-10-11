using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Files;
using SkEditor.Views.Windows.Settings.Personalization;

namespace SkEditor.Views.Windows.Settings;

public partial class PersonalizationPage : UserControl
{
    public PersonalizationPage()
    {
        InitializeComponent();

        AssignCommands();
    }

    private void AssignCommands()
    {
        ThemePageButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(ThemePage)));
        SyntaxPageButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(FileSyntaxes)));

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