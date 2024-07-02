using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.API;
using System.Collections.Generic;

namespace SkEditor.ViewModels;

public partial class FileTypeSelectionViewModel : ObservableObject
{

    private FileTypeData? _selectedFileType;
    public FileTypeData? SelectedFileType
    {
        get => _selectedFileType;
        set
        {
            SetProperty(ref _selectedFileType, value);
            IsFileTypeSelected = value != null;
        }
    }

    [ObservableProperty] private List<FileTypeData> _fileTypes;

    [ObservableProperty] private bool _isFileTypeSelected;
    [ObservableProperty] private bool _rememberSelection;

}