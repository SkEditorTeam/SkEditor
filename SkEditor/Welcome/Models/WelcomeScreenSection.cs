using System.Collections.Generic;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Welcome;

public record WelcomeScreenSection(TranslatableString Title, List<WelcomeScreenElement> Entries);