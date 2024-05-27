using SkEditor.Parser;
using SkEditor.Parser.Elements;

namespace SkEditor.Utilities.Parser.Elements;

public class StructEvent : Element
{
    public override void Load(Node node)
    {
        var section = (SectionNode) node;
        ElementParser.ParseNodes(section.Children);
    }

    public static bool Parse(Node node)
    {
        return node is { IsSection: true, Indent: 0 };
    }
}