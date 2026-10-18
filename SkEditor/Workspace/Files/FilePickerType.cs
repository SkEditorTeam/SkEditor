using TranslatableString = SkEditor.Core.Localization.TranslatableString;

namespace SkEditor.Workspace.Files;

public record FilePickerType(TranslatableString Name, params string[] Patterns);