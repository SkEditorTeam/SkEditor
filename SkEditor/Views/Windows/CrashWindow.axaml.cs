using System;
using SkEditor.Utilities;
using SkEditor.Views.Controls;

namespace SkEditor.Views.Windows;

public partial class CrashWindow : SkEditorWindow
{
    public CrashWindow(string exception)
    {
        InitializeComponent();

        CrashStackTrace.Text = exception;
        AssignCommands();
    }

    [Obsolete("This constructor is for designer use only.", true)]
    public CrashWindow() => InitializeComponent();

    public void AssignCommands()
    {
        DiscordButton.Click += (_, _) => OpenDiscord();
        CloseButton.Click += (_, _) => Close();
    }

    public static void OpenDiscord()
    {
        LinkOpener.OpenLink("https://discord.skeditor.dev");
    }
}