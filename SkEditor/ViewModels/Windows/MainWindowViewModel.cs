using SkEditor.ViewModels.Controls;

namespace SkEditor.ViewModels.Windows;

public class MainWindowViewModel(MainMenuViewModel mainMenu)
{
    public MainMenuViewModel MainMenu { get; } = mainMenu;
}