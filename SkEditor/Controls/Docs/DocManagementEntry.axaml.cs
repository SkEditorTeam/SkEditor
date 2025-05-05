using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities;
using SkEditor.Utilities.Docs.Local;

namespace SkEditor.Controls.Docs;

public partial class DocManagementEntry : UserControl
{
    private readonly LocalDocEntry _entry;

    public DocManagementEntry(LocalDocEntry entry)
    {
        InitializeComponent();
        _entry = entry;

        LoadVisuals();
        AssignCommands();
    }

    public void LoadVisuals()
    {
        NameBlock.Text = _entry.Name;
        Description.Text = Translation.Get("LocalDocsManagerWindowEntryDescription", _entry.DocType.ToString(),
            _entry.OriginalProvider.ToString());
    }

    public void AssignCommands()
    {
        DeleteButton.Command = new RelayCommand(() =>
        {
            _ = LocalProvider.Get().RemoveElement(_entry);
            SettingsExpander? parent = Parent as SettingsExpander;
            parent.Items.Remove(this);
            if (parent.Items.Count != 0)
            {
                return;
            }

            StackPanel? parentParent = parent.Parent as StackPanel;
            parentParent.Children.Remove(parent);
        });
    }
}