using System.IO;

namespace SkEditor.Configuration.Abstractions;

internal interface IBinarySettingHandler
{
    void Load(BinaryReader reader);
    void Save(BinaryWriter writer);
}