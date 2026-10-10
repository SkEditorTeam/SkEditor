using SkEditor.Matcher.Models;
using SkEditor.Matcher.Registries;

namespace SkEditor.Matcher;

public interface ISkriptSyntaxService
{
    TypeRegistry TypeRegistry { get; }

    void Load();
    void LoadFromStream(Stream jsonStream);
    IEnumerable<(SyntaxElement Element, SyntaxPattern Pattern)> GetSyntaxByType(SyntaxElementDto.SyntaxType type);

    Task InitializationTask { get; }
}