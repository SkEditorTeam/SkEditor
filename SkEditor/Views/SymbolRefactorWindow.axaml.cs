using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.InternalAPI;
using SkEditor.Utilities.Parser;

namespace SkEditor.Views;

public partial class SymbolRefactorWindow : AppWindow
{
    public INameableCodeElement Element { get; }
    public SymbolRefactorWindow(INameableCodeElement element)
    {
        InitializeComponent();
        Element = element;

        RenameText.Text = Translation.Get("RefactorWindowRefactorBoxName", element.GetNameDisplay());
        NameBox.Text = element.Name;
        RefactorButton.Command = new RelayCommand(Refactor);
    }

    private void Refactor()
    {
        Element.Rename(NameBox.Text);
        Close();

        AddonLoader.GetCoreAddon().ParserPanel.Panel.ParseCurrentFile();
    }
}