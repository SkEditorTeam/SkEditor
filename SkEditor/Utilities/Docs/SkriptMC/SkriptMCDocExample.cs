namespace SkEditor.Utilities.Docs.SkriptMC;

public class SkriptMcDocExample : IDocumentationExample
{
    public required string Example { get; set; }

    public string Author
    {
        get => "SkriptMC";
        set { }
    }

    public string Votes
    {
        get => "0";
        set { }
    }
}