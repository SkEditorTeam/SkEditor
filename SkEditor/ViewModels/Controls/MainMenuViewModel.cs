using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using FluentIcons.Common;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Projects;
using SkEditor.Views.Windows;

namespace SkEditor.ViewModels.Controls;

public partial class MainMenuViewModel : ObservableObject
{
    [ObservableProperty]
    private IEnumerable<MainMenuItem> _items = new List<MainMenuItem>();

    public MainMenuViewModel()
    {
        MainMenuItem newFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderNew, Symbol.Document)
            .WithCommand(FileHandler.NewFile);

        MainMenuItem openFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderOpen, Symbol.DocumentArrowUp)
            .WithCommand(FileHandler.OpenFile);

        MainMenuItem openFolder = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderOpenFolder, Symbol.Folder)
            .WithCommand(() => ProjectOpener.OpenProject());

        MainMenuItem saveFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSave, Symbol.Save)
            .WithCommand(FileHandler.SaveFile);

        MainMenuItem saveAsFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAs, Symbol.SaveEdit)
            .WithCommand(FileHandler.SaveAsFile);

        MainMenuItem saveAllFiles = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAll, Symbol.SaveMultiple)
            .WithCommand(FileHandler.SaveAllFiles);

        MainMenuItem publishFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderPublish, Symbol.Share)
            .WithCommand(OpenWindowDialog(new PublishWindow()));

        Items = new List<MainMenuItem>
        {
            MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderFile).WithItems([
                newFile,
                openFile,
                openFolder,
                saveFile,
                saveAsFile,
                saveAllFiles,
                publishFile
            ])
        };
    }

    private static AsyncRelayCommand OpenWindowDialog(AppWindow window)
    {
        return new AsyncRelayCommand(async () => { await window.ShowDialogOnMainWindow(); });
    }
}

public record MainMenuItem
{
    public TranslationKey? HeaderKey { get; init; }
    public Symbol? IconSymbol { get; init; }
    public IRelayCommand? Command { get; init; }
    public IEnumerable<MainMenuItem>? SubItems { get; init; }
    public string? InputGestureText { get; init; }
    public bool IsSeparator { get; init; }

    /// <remarks>
    /// Use the static builders instead of this constructor.
    /// </remarks>
    protected MainMenuItem()
    {
    }

    public static MainMenuItem Separator => new() { IsSeparator = true };

    public static MainMenuItem HeaderOnly(TranslationKey headerKey)
    {
        return new MainMenuItem
        {
            HeaderKey = headerKey
        };
    }

    public static MainMenuItem IconOnly(Symbol iconSymbol)
    {
        return new MainMenuItem
        {
            IconSymbol = iconSymbol
        };
    }

    public static MainMenuItem HeaderWithIcon(TranslationKey headerKey, Symbol iconSymbol)
    {
        return new MainMenuItem
        {
            HeaderKey = headerKey,
            IconSymbol = iconSymbol
        };
    }

    public MainMenuItem WithItem(MainMenuItem subItem)
    {
        var subItems = SubItems is not null ? new List<MainMenuItem>(SubItems) : new List<MainMenuItem>();
        subItems.Add(subItem);
        return this with { SubItems = subItems };
    }

    public MainMenuItem WithItems(IEnumerable<MainMenuItem> subItems)
    {
        var newSubItems = SubItems is not null ? new List<MainMenuItem>(SubItems) : new List<MainMenuItem>();
        newSubItems.AddRange(subItems);
        return this with { SubItems = newSubItems };
    }

    public MainMenuItem WithIcon(Symbol iconSymbol)
    {
        return this with { IconSymbol = iconSymbol };
    }

    public MainMenuItem WithCommand(IRelayCommand command)
    {
        return this with { Command = command };
    }

    public MainMenuItem WithCommand(Action execute)
    {
        return this with { Command = new RelayCommand(execute) };
    }

    public MainMenuItem WithCommand(Func<Task> execute)
    {
        return this with { Command = new AsyncRelayCommand(execute) };
    }

    public MainMenuItem WithInputGesture(string inputGestureText)
    {
        return this with { InputGestureText = inputGestureText };
    }
}