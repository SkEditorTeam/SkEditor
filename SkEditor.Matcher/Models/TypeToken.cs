namespace SkEditor.Matcher.Models;

public readonly record struct TypeToken(int Value)
{
    public int TypeId => Value & 0x7FFFFFFF;
    public bool IsPlural => (Value & unchecked((int)0x80000000)) != 0;

    public TypeToken(int typeId, bool isPlural)
        : this(isPlural ? typeId | unchecked((int)0x80000000) : typeId)
    {
    }

    public TypeToken WithPlural(bool isPlural)
    {
        return new TypeToken(TypeId, isPlural);
    }

    public const int ObjectTypeId = 0;
    public const int StringTypeId = 1;
    public const int NumberTypeId = 2;
    public const int BooleanTypeId = 3;

    public static readonly TypeToken Object = new(ObjectTypeId);
    public static readonly TypeToken String = new(StringTypeId);
    public static readonly TypeToken Number = new(NumberTypeId);
    public static readonly TypeToken Boolean = new(BooleanTypeId);
}