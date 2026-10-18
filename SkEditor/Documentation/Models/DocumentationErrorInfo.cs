using FluentIcons.Common;
using TranslatableString = SkEditor.Core.Localization.TranslatableString;

namespace SkEditor.Documentation.Models;

public record DocumentationErrorInfo(
    TranslatableString Title,
    TranslatableString Message,
    Symbol Icon,
    bool CanRetry = true,
    bool IsApiKeyMissing = false
);