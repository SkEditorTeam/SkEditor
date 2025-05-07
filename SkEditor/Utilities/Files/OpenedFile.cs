using System.Collections.Generic;
using Avalonia.Controls;
using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Parser;

namespace SkEditor.Utilities.Files;

public class OpenedFile
{
    public TabViewItem? TabViewItem { get; set; }

    #region Text Files Properties

    public CodeParser? Parser => this["Parser"] as CodeParser;
    public TextEditor? Editor { get; set; }
    public string? Path { get; set; }
    public bool IsNewFile { get; set; }

    private bool _saved;

    public bool IsSaved
    {
        get => _saved;
        set
        {
            _saved = value;
            if (TabViewItem != null)
            {
                TabViewItem.Header = Header;
            }
        }
    }

    #endregion

    #region Custom Tabs Properties

    public bool IsCustomTab => Editor == null;
    public Control? CustomControl => IsCustomTab ? TabViewItem?.Content as Control : null;
    public string? CustomName = null;

    #endregion

    #region Accessors

    public bool IsEditor => Editor != null;
    public string? Name => Path == null ? CustomName : System.IO.Path.GetFileName(Path);

    public string Header =>
        Name + (IsSaved || (SkEditorAPI.Core.GetAppConfig().IsAutoSaveEnabled && Path != null) ? "" : " •");

    #endregion

    #region Custom Data

    public List<CustomFileData> CustomData { get; } = [];

    public object? this[string key]
    {
        get
        {
            CustomFileData? data = CustomData.Find(d => d.Key == key);
            return data?.Value;
        }
        set
        {
            CustomFileData? data = CustomData.Find(d => d.Key == key);
            if (data != null)
            {
                CustomData.Remove(data);
            }
            
            if (value == null) return;
            CustomData.Add(new CustomFileData(key, value));
        }
    }

    #endregion
}