using System.IO;

namespace SkEditor.Utilities.Settings;

internal interface IBinarySettingHandler
{
    void Load(BinaryReader reader);
    void Save(BinaryWriter writer);
}