using System.Collections.Generic;
using SkEditor.Core.Localization;

namespace SkEditor.Welcome.Models;

public record WelcomeScreenSection(TranslatableString Title, List<WelcomeScreenElement> Entries);