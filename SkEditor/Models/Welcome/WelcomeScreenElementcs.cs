using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Localization;
using SkEditor.ViewModels.Pages;

namespace SkEditor.Models.Welcome;

public record WelcomeScreenElement(TranslatableString Name, IRelayCommand Command, IconSource? Icon);