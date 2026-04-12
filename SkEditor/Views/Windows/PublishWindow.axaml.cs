using Avalonia.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.Utilities.Styling;
using SkEditor.ViewModels.Windows;

namespace SkEditor.Views.Windows;

public partial class PublishWindow : FAAppWindow
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
}