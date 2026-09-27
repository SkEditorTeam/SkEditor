using System;

namespace SkEditor.Utilities.Docs;

public class DocumentationException : Exception
{
    public bool ApiKeyMissing { get; set; }
    public bool ApiKeyInvalid { get; set; }
}