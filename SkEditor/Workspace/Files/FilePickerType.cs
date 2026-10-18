using SkEditor.Core.Localization;

namespace SkEditor.Workspace.Files;

public record FilePickerType(TranslatableString Name, params string[] Patterns);