using FluentIcons.Common;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Documentation;

public record DocumentationErrorInfo(
    TranslatableString Title,
    TranslatableString Message,
    Symbol Icon,
    bool CanRetry = true,
    bool IsApiKeyMissing = false
);