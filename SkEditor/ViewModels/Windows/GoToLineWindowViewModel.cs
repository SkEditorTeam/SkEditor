using System;
using System.Diagnostics.CodeAnalysis;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services.Files;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.ViewModels.Windows;

public partial class GoToLineWindowViewModel : ObservableObject
{
    private readonly ITabService _tabs;

    [ObservableProperty]
    public partial string Input { get; set; } = string.Empty;

    public GoToLineWindowViewModel(ITabService tabs)
    {
        _tabs = tabs;

        if (_tabs.SelectedTab is not FileTabViewModel fileTab)
        {
            return;
        }

        TextLocation location = fileTab.Document.GetLocation(fileTab.CaretOffset);
        Input = $"{location.Line}:{location.Column}";
    }

    [RelayCommand]
    public void Execute()
    {
        if (string.IsNullOrWhiteSpace(Input))
        {
            CloseRequested?.Invoke();
            return;
        }

        if (_tabs.SelectedTab is not FileTabViewModel fileTab)
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

        if (_tabs.SelectedTab is not FileTabViewModel fileTab)
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

    public event Action? CloseRequested;
}