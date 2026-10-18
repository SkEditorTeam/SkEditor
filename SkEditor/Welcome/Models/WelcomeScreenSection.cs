using System.Collections.Generic;
using TranslatableString = SkEditor.Core.Localization.TranslatableString;

namespace SkEditor.Welcome.Models;

public record WelcomeScreenSection(TranslatableString Title, List<WelcomeScreenElement> Entries);