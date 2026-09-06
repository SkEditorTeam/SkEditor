using System.Collections.Frozen;
using SkEditor.Matcher.Models;

namespace SkEditor.Matcher.Registries;

public class TypeRegistry
{
    private readonly FrozenDictionary<string, TypeToken> _stringToToken;
    public int TypeCount { get; }
    private readonly string[] _singularTypeNames;
    private readonly string[] _pluralTypeNames;

    public TypeRegistry(List<ClassInfoDto> classInfos)
    {
        Dictionary<string, int> stringToId = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, TypeToken> stringToTokenMap = new(StringComparer.OrdinalIgnoreCase);

        stringToId["object"] = TypeToken.ObjectTypeId;
        stringToId["string"] = TypeToken.StringTypeId;
        stringToId["number"] = TypeToken.NumberTypeId;
        stringToId["boolean"] = TypeToken.BooleanTypeId;

        int nextId = 4;
        foreach (ClassInfoDto classInfo in classInfos.Where(classInfo => !stringToId.ContainsKey(classInfo.CodeName)))
        {
            stringToId[classInfo.CodeName] = nextId++;
        }

        TypeCount = nextId;
        _singularTypeNames = new string[TypeCount];
        _pluralTypeNames = new string[TypeCount];

        RegisterBuiltInType("object", TypeToken.ObjectTypeId, stringToTokenMap);
        RegisterBuiltInType("string", TypeToken.StringTypeId, stringToTokenMap);
        RegisterBuiltInType("number", TypeToken.NumberTypeId, stringToTokenMap);
        RegisterBuiltInType("boolean", TypeToken.BooleanTypeId, stringToTokenMap);

        foreach (ClassInfoDto classInfo in classInfos)
        {
            int id = stringToId[classInfo.CodeName];
            TypeToken singularToken = new(id, false);
            TypeToken pluralToken = new(id, true);

            stringToTokenMap.TryAdd(classInfo.CodeName, singularToken);
            if (!string.IsNullOrEmpty(classInfo.Plural))
            {
                stringToTokenMap.TryAdd(classInfo.Plural, pluralToken);
            }

            _singularTypeNames[id] = classInfo.CodeName;
            _pluralTypeNames[id] = classInfo.Plural;
        }

        _stringToToken = stringToTokenMap.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private void RegisterBuiltInType(string name, int fixedId, Dictionary<string, TypeToken> map)
    {
        _singularTypeNames[fixedId] = name;
        _pluralTypeNames[fixedId] = name + "s";

        map.TryAdd(name, new TypeToken(fixedId, false));
        map.TryAdd(name + "s", new TypeToken(fixedId, true));
    }

    public bool TryParseToken(string typeName, out TypeToken token)
    {
        return _stringToToken.TryGetValue(typeName, out token);
    }

    public string GetName(TypeToken token)
    {
        return token.IsPlural ? _pluralTypeNames[token.TypeId] : _singularTypeNames[token.TypeId];
    }
}