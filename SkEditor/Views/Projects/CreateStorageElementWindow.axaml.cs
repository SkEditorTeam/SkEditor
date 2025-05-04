using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.Utilities;
using SkEditor.Utilities.Projects.Elements;
using SkEditor.Utilities.Styling;

namespace SkEditor.Views.Projects;

public partial class CreateStorageElementWindow : AppWindow
{
    public Folder Folder;
    public bool IsFile;

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
    }

    private void Create()
    {
        string? input = NameTextBox.Text;
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