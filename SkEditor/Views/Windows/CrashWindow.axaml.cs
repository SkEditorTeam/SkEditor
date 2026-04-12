using System;
using Avalonia.Input;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;

namespace SkEditor.Views.Windows;

public partial class CrashWindow : FAAppWindow
{
    public CrashWindow(string exception)
    {
        InitializeComponent();
        Focusable = true;

        CrashStackTrace.Text = exception;
        AssignCommands();
    }

    [Obsolete("This constructor is for designer use only.", true)]
    public CrashWindow() => InitializeComponent();

    public void AssignCommands()
    {
        DiscordButton.Click += (_, _) => OpenDiscord();
        CloseButton.Click += (_, _) => Close();
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };
    }

    public static void OpenDiscord()
    {
        SkEditorAPI.Core.OpenLink("https://discord.skeditor.dev");
    }
}