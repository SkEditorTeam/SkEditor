using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.Utilities;
using SkEditor.Utilities.InternalAPI;
using SkEditor.Utilities.Parser;

namespace SkEditor.Views;

public partial class SymbolRefactorWindow : AppWindow
{
    public SymbolRefactorWindow(INameableCodeElement element)
    {
        InitializeComponent();
        Focusable = true;

        Element = element;

        RenameText.Text = Translation.Get("RefactorWindowRefactorBoxName", element.GetNameDisplay());
        NameBox.Text = element.Name;
        RefactorButton.Command = new RelayCommand(Refactor);

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };
    }

    public INameableCodeElement Element { get; }

    private void Refactor()
    {
        Element.Rename(NameBox.Text);
        Close();

        AddonLoader.GetCoreAddon().ParserPanel.Panel.ParseCurrentFile();
    }
}