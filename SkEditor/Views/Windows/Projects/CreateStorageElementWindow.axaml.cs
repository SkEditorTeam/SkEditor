using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.Utilities;
using SkEditor.Utilities.Projects.Elements;
using SkEditor.Utilities.Styling;

namespace SkEditor.Views.Windows.Projects;

public partial class CreateStorageElementWindow : AppWindow
{
    public Folder Folder { get; }
    public bool IsFile { get; }

    public CreateStorageElementWindow(Folder folder, bool isFile)
    {
        Folder = folder;
        IsFile = isFile;

        InitializeComponent();
        WindowStyler.Style(this);
        Focusable = true;

        FileNameTextBlock.Text = Translation.Get(isFile ? "ProjectCreateFileName" : "ProjectCreateFolderName");
        FileTemplateTextBlock.Text = Translation.Get("ProjectCreateTemplate");

        CreateButton.Command = new RelayCommand(Create);
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };
        
        var placeholder =
            Translation.Get(isFile ? "ProjectCreatePlaceholderFileName" : "ProjectCreatePlaceholderFolderName");

        NameTextBox.Text = placeholder + (isFile ? ".sk" : "");
        
        Opened += (_, _) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                NameTextBox.Focus();
                NameTextBox.SelectionStart = 0;
                NameTextBox.SelectionEnd = placeholder.Length;
            });
        };
    }

    private void Create()
    {
        string? input = NameTextBox.Text;
        if (string.IsNullOrWhiteSpace(input))
        {
            ErrorBox.Text = "The name cannot be empty.";
            return;
        }
        string? error = Folder.ValidateCreationName(input);
        if (error != null)
        {
            ErrorBox.Text = error;
            return;
        }

        if (IsFile)
        {
            Folder.CreateFile(input);
        }
        else
        {
            Folder.CreateFolder(input);
        }

        Close();
    }
}