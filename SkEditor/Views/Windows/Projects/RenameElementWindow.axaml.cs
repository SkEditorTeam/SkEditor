using System;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Projects.Elements;
using SkEditor.Utilities.Styling;

namespace SkEditor.Views.Windows.Projects;

public partial class RenameElementWindow : AppWindow
{
    public RenameElementWindow(StorageElement element)
    {
        InitializeComponent();
        Focusable = true;

        Element = element;
        NameBox.Text = element.Name;

        WindowStyler.Style(this);

        RenameButton.Command = new RelayCommand(Rename);
        CancelButton.Command = new RelayCommand(Close);

        KeyDown += (_, e) =>
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.Enter:
                    Rename();
                    break;
            }
        };

        Opened += (_, _) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                NameBox.Focus();
                NameBox.SelectionStart = 0;
                NameBox.SelectionEnd = element.Name.Length;
            });
        };
    }

    [Obsolete("This constructor is for designer use only.", true)]
    public RenameElementWindow()
    {
        InitializeComponent();
        Element = null!;
    }

    public StorageElement Element { get; }

    private void Rename()
    {
        string? input = NameBox.Text;
        if (string.IsNullOrWhiteSpace(input))
        {
            ErrorBox.Text = Translation.Get("ProjectRenameErrorNameEmpty");
            return;
        }

        string? error = Element.ValidateName(input);
        if (error != null)
        {
            ErrorBox.Text = error;
            return;
        }

        Element.RenameElement(input);
        Close();
    }
}