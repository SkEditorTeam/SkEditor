using System;

namespace SkEditor.Utilities.Docs.Local;

[Serializable]
public class LocalDocExample(IDocumentationExample other) : IDocumentationExample
{
    public string Example { get; set; } = other.Example;
    public string Author { get; set; } = other.Author;
    public string Votes { get; set; } = other.Votes;
}