using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Files;

public record FilePickerType(TranslatableString Name, params string[] Patterns);