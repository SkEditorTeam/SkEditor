using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Parser;

namespace SkEditor.Utilities.Files;

public class OpenedFile
{

    public TextEditor? Editor { get; set; }
    public string Path { get; set; }
    public TabViewItem TabViewItem { get; set; }
    public CodeParser? Parser { get; set; }

}