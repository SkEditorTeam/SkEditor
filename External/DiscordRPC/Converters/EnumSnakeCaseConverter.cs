using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

// Custom snake_case naming policy for enums
/// <summary>
/// 
/// </summary>
public class EnumSnakeCaseConverter : JsonNamingPolicy
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override string ConvertName(string name)
    {
        // Simple snake_case implementation
        return string.Concat(
                             name.Select((c, i) =>
                                             i > 0 && char.IsUpper(c) ? "_" + char.ToLower(c) : char.ToLower(c).ToString()
                                        )
                            );
    }
}