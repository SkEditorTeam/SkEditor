using System;

namespace SkEditor.Utilities.Docs.Local;

[Serializable]
public class LocalDocExample : IDocumentationExample
{
    public LocalDocExample() { }

    public LocalDocExample(IDocumentationExample other)
    {
        Example = other.Example;
        Author = other.Author;
        Votes = other.Votes;
    }

    public string Example { get; set; }
    public string Author { get; set; }
    public string Votes { get; set; }
}