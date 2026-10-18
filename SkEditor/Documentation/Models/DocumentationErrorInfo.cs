using FluentIcons.Common;
using SkEditor.Core.Localization;

namespace SkEditor.Documentation.Models;

public record DocumentationErrorInfo(
    TranslatableString Title,
    TranslatableString Message,
    Symbol Icon,
    bool CanRetry = true,
    bool IsApiKeyMissing = false
);