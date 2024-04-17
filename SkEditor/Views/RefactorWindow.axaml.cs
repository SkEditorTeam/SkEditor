using AvaloniaEdit;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkEditor.Views;
public partial class RefactorWindow : AppWindow
{

    public RefactorWindow()
    {
        InitializeComponent();

        ApplyButton.Command = new RelayCommand(Apply);
    }

    private async void Apply()
    {
        if (RemoveCommentsCheckBox.IsChecked == true) await RemoveComments();
        if (TabsToSpacesCheckBox.IsChecked == true) await TabsToSpaces();
        if (SpacesToTabsCheckBox.IsChecked == true) await SpacesToTabs();

        Close();
    }

    private static Task RemoveComments()
    {
        TextEditor textEditor = ApiVault.Get().GetTextEditor();
        var linesToStay = textEditor.Document.Lines.Where(x => !GetText(x).Trim().StartsWith('#')).ToList();

        StringBuilder builder = new();
        linesToStay.ForEach(x => builder.AppendLine(GetText(x)));
        textEditor.Document.Text = builder.ToString();

        return Task.CompletedTask;
    }

    private static Task TabsToSpaces()
    {
        TextEditor textEditor = ApiVault.Get().GetTextEditor();
        var lines = textEditor.Document.Lines;

        lines.Where(line => GetText(line).StartsWith("\t")).ToList().ForEach(line =>
        {
            int tabs = GetText(line).TakeWhile(x => x == '\t').Count();
            textEditor.Document.Replace(line.Offset, tabs, new string(' ', tabs * 4));
        });
        return Task.CompletedTask;
    }

    private static Task SpacesToTabs()
    {
        TextEditor textEditor = ApiVault.Get().GetTextEditor();
        int tabSize = GetTabSize();
        var lines = textEditor.Document.Lines;
        lines.Where(line => GetText(line).StartsWith(" ")).ToList().ForEach(line =>
        {
            int spaces = GetText(line).TakeWhile(x => x == ' ').Count();
            if (spaces % tabSize == 0)
            {
                textEditor.Document.Replace(line.Offset, spaces, new string('\t', spaces / tabSize));
            }
        });

        return Task.CompletedTask;
    }

    private static int GetTabSize()
    {
        TextEditor textEditor = ApiVault.Get().GetTextEditor();
        var lines = textEditor.Document.Lines;
        int tabSize = 4;

        var line = lines.FirstOrDefault(line => GetText(line).StartsWith(" "));

        if (line != null) tabSize = GetText(line).TakeWhile(x => x == ' ').Count();

        return tabSize;
    }

    private static string GetText(DocumentLine line)
    {
        TextEditor textEditor = ApiVault.Get().GetTextEditor();
        return textEditor.Document.GetText(line.Offset, line.Length);
    }
}
