using System;
using System.IO;

namespace SkEditor.Utilities.Settings;

internal class BinarySettingHandler<T>(
    Func<T> getter,
    Action<T> setter,
    Func<BinaryReader, T> readerFunc,
    Action<BinaryWriter, T> writerFunc) : IBinarySettingHandler
{
    public void Load(BinaryReader reader)
    {
        setter(readerFunc(reader));
    }

    public void Save(BinaryWriter writer)
    {
        writerFunc(writer, getter());
    }
}