using System;
using System.Diagnostics.CodeAnalysis;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Tabs;

namespace SkEditor.ViewModels.Windows;

public partial class GoToLineWindowViewModel(ITabService tabs) : ObservableObject
{
    [ObservableProperty]
    public partial string Input { get; set; } = string.Empty;

    public event Action? CloseRequested;

    [RelayCommand]
    public void Execute()
    {
        if (string.IsNullOrWhiteSpace(Input))
        {
            CloseRequested?.Invoke();
            return;
        }

        if (tabs.SelectedTab is not FileTab fileTab)
        {
            return;
        }

        if (!TryParseLocation(out TextLocation? location))
        {
            UpdateInput();
            return;
        }

        fileTab.RequestGoToLocation(location.Value);

        CloseRequested?.Invoke();
    }

    private bool TryParseLocation([NotNullWhen(true)] out TextLocation? location)
    {
        location = null;
        if (Input.Contains(':'))
        {
            string[] parts = Input.Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int line) || !int.TryParse(parts[1], out int column))
            {
                return false;
            }

            location = new TextLocation(line, column);
        }
        else
        {
            if (!int.TryParse(Input, out int lineNumber))
            {
                return false;
            }

            location = new TextLocation(lineNumber, 0);
        }

        return true;
    }

    private void UpdateInput()
    {
        if (tabs.SelectedTab is not FileTab fileTab)
        {
            Input = string.Empty;
            return;
        }

        int offset = fileTab.CaretOffset;
        TextLocation location = fileTab.Document.GetLocation(offset);
        Input = $"{location.Line}:{location.Column}";
    }
}