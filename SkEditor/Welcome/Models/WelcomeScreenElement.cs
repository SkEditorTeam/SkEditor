using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.Core.Localization;

namespace SkEditor.Welcome.Models;

public record WelcomeScreenElement(
    TranslatableString Name,
    IRelayCommand Command,
    FAIconSource? Icon,
    bool? IsEnabled = true)
{
    public static WelcomeScreenElement Create(TranslationKey nameKey, IRelayCommand command, FAIconSource? icon = null)
    {
        return new WelcomeScreenElement(new TranslatableString(nameKey), command, icon);
    }

    public WelcomeScreenElement Disable()
    {
        return this with { IsEnabled = false };
    }
}