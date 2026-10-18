using System;

namespace SkEditor.Documentation.Models;

public class DocumentationException : Exception
{
    public bool ApiKeyMissing { get; set; }
    public bool ApiKeyInvalid { get; set; }
}