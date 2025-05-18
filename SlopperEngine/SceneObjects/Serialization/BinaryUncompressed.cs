using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using SlopperEngine.Core;

namespace SlopperEngine.SceneObjects.Serialization;

public partial class SerializedObjectTree
{
    public void SaveBinaryUncompressed(string filepath)
    {
        using var stream = new FileStream(Assets.GetPath(filepath), FileMode.Create, FileAccess.Write);
        SaveBinaryUncompressed(stream);
    }

    public void SaveBinaryUncompressed(Stream stream)
    {
        stream.WriteByte((byte)'S'); // header for SlopBinaryUncompressed
        stream.WriteByte((byte)'L');
        stream.WriteByte((byte)'B');
        stream.WriteByte((byte)'U');
        using StreamWriter w = new(stream, Encoding.Unicode);

        WriteIntToStream(1); // version

        Dictionary<Type, int> allNamedTypes = new();
        int typeCount = _indexedTypes.Count;
        for (int i = 0; i < typeCount; i++)
        {
            var type = _indexedTypes[i];
            allNamedTypes.TryAdd(type.Type, -1);
            foreach (var (method, originatingType) in type.OnSerializeMethods)
            {
                if (originatingType != null)
                    allNamedTypes.TryAdd(originatingType, -1);
            }
        }
        var types = allNamedTypes.ToArray();

        int typeIndex = 0;
        WriteIntToStream(types.Length);
        foreach ((var type, var _) in types)
        {
            allNamedTypes[type] = typeIndex;
            if (!TryWriteStringToStream(type.FullName)) // every type that is ever contained within this file
                System.Console.WriteLine($"Type {type} couldnt be written to a string.");

            typeIndex++;
        }

        WriteIntToStream(typeCount); // amount of serialized types

        for (int i = 0; i < typeCount; i++)
        {
            var type = _indexedTypes[i];
            var index = allNamedTypes[type.Type];
            WriteIntToStream(type.Fields.Count); // amount of fields
            foreach (var f in type.Fields)
            {

            }
        }

        bool TryWriteStringToStream(string? value)
        {
            if (value == null)
            {
                WriteIntToStream(0);
                return false;
            }
            WriteIntToStream(value.Length);
            w.Write(value);
            w.Flush();
            return true;
        }
        void WriteIntToStream(int value)
        {
            Span<byte> binaryInt = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(binaryInt, value);
            stream.Write(binaryInt);
        }
    }
}