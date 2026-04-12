using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Welcome;

public record WelcomeScreenElement(TranslatableString Name, IRelayCommand Command, FAIconSource? Icon)
{
    public static WelcomeScreenElement Create(TranslationKey nameKey, IRelayCommand command, FAIconSource? icon = null)
    {
        return new WelcomeScreenElement(new TranslatableString(nameKey), command, icon);
    }
}