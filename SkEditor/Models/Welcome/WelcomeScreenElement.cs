using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Welcome;

public record WelcomeScreenElement(TranslatableString Name, IRelayCommand Command, IconSource? Icon)
{
    public static WelcomeScreenElement Create(TranslationKey nameKey, IRelayCommand command, IconSource? icon = null)
    {
        return new WelcomeScreenElement(new TranslatableString(nameKey), command, icon);
    }
}