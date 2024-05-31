using System.Collections.Generic;
using System.IO.Compression;
using Avalonia.Controls;
using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Parser;

namespace SkEditor.Utilities.Files;

public class OpenedFile
{

    #region Text Files Properties

    public TextEditor? Editor { get; set; }
    public CodeParser? Parser { get; set; }
    public string? Path { get; set; }

    private bool _saved;
    public bool IsSaved {
        get => _saved;
        set
        {
            _saved = value;
            TabViewItem.Header = Header;
        }
    }

    #endregion

    #region Custom Tabs Properties

    public bool IsCustomTab { get; set; } = false;

    #endregion

    public TabViewItem TabViewItem { get; set; }
    public Dictionary<string, object> CustomData { get; } = new();

    #region Accessors

    public bool IsEditor => Editor != null;
    public string? Name => Path == null ? null : System.IO.Path.GetFileNameWithoutExtension(Path);
    public string? Header => Name + (IsSaved ? "" : " •");

    #endregion
}