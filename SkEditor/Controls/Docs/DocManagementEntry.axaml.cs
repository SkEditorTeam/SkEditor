using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Docs.Local;

namespace SkEditor.Controls.Docs;

public partial class DocManagementEntry : UserControl
{
    
    private LocalDocEntry _entry;
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
        Description.Text = $"{_entry.DocType.ToString()} provided by {_entry.OriginalProvider.ToString()}";
    }

    public void AssignCommands()
    {
        DeleteButton.Command = new RelayCommand(() =>
        {
            LocalProvider.Get().RemoveElement(_entry);
            var parent = this.Parent as SettingsExpander;
            parent.Items.Remove(this);
            if (parent.Items.Count != 0)
                return;

            var parentParent = parent.Parent as StackPanel;
            parentParent.Children.Remove(parent);
        });
    }
}