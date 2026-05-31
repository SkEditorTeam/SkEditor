namespace SkEditor.Models.Documentation;

public enum DocumentationSyntaxType
{
    All,
    Event,
    Effect,
    Expression,
    Condition,
    Type,
    Structure,
    Section,
    Function
}

public static class DocumentationSyntaxTypeExtensions
{
    extension(DocumentationSyntaxType)
    {
        public static DocumentationSyntaxType FromString(string type)
        {
            type = type.ToLowerInvariant();
            if (type.EndsWith('s'))
            {
                type = type[..^1];
            }

            return type switch
            {
                "event" => DocumentationSyntaxType.Event,
                "effect" => DocumentationSyntaxType.Effect,
                "expression" => DocumentationSyntaxType.Expression,
                "condition" => DocumentationSyntaxType.Condition,
                "type" or "class" or "classe" => DocumentationSyntaxType.Type,
                "structure" => DocumentationSyntaxType.Structure,
                "section" => DocumentationSyntaxType.Section,
                "function" => DocumentationSyntaxType.Function,
                _ => DocumentationSyntaxType.Expression
            };
        }
    }
}