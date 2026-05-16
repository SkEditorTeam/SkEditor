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
            return;
        }

        fileTab.RequestGoToLocation(location.Value);

        CloseRequested?.Invoke();
    }

    private bool TryParseLocation([NotNullWhen(true)] out TextLocation? location)
    {
        location = null;

        int line;
        int column;

        if (Input.Contains(':'))
        {
            string[] parts = Input.Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[0], out line) || !int.TryParse(parts[1], out column))
            {
                return false;
            }
        }
        else
        {
            if (!int.TryParse(Input, out line))
            {
                return false;
            }

            column = 1;
        }

        if (tabs.SelectedTab is not FileTab fileTab)
        {
            return false;
        }

        if (line < 1 || column < 1)
        {
            return false;
        }

        line = Math.Clamp(line, 1, fileTab.Document.LineCount);
        column = Math.Clamp(column, 1, fileTab.Document.GetLineByNumber(line).Length + 1);

        location = new TextLocation(line, column);
        return true;
    }
}