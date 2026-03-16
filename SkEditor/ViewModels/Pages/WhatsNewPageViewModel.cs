using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Icons;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.ViewModels.Pages;

public class WhatsNewPageViewModel : ObservableObject
{
    public string Version { get; } = $"v{SkEditorAPI.Core.GetInformationalVersion()}";

    public List<FeaturedItem> FeaturedItems { get; } =
    [
        FeaturedItem.Create(Symbol.Rocket, "New Architecture",
            "A significant portion of the codebase has been rewritten to introduce a superior, highly extensible architecture, greatly enhancing the developer experience."),
        FeaturedItem.Create(Symbol.PaintBrush, "Revamped Interface",
            "Numerous UI enhancements, a modern toast notification system, a redesigned settings view, and much more!"),
        FeaturedItem.Create(Symbol.Folder, "Overhauled Project System",
            "Projects have finally received a major update. Completely rewritten from the ground up, they are now significantly faster and far more practical."),
        FeaturedItem.Create("RemoteDesktopIcon", "SFTP Support",
            "At long last, SkEditor features built-in SFTP integration! You can now connect directly to remote servers and edit files without ever leaving the editor.")
    ];

    public List<string> Changes { get; } =
    [
        "Added a new 'What's New' page to highlight the latest features and improvements in SkEditor.",
        "Implemented a modern toast notification system to provide users with timely updates and feedback.",
        "Redesigned the settings view for improved usability and a more cohesive user experience.",
        "Enhanced the overall user interface with various visual improvements and optimizations."
    ];

    public record FeaturedItem(IconSource Icon, string Title, string Description)
    {
        public static FeaturedItem Create(Symbol icon, string title, string description)
        {
            return new FeaturedItem(new SymbolIconSource { Symbol = icon }, title, description);
        }

        public static FeaturedItem Create(string pathIconName, string title, string description)
        {
            return new FeaturedItem(Icons.GetPathIcon(pathIconName), title, description);
        }
    }
}