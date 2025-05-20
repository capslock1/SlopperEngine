using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using SlopperEngine.Core;
using SlopperEngine.Core.Collections;
using SlopperEngine.Core.Serialization;

namespace SlopperEngine.SceneObjects.Serialization;

public partial class SerializedObjectTree
{
    const int _BinaryUncompressedVersion = 1;
    public static SerializedObjectTree? LoadBinaryUncompressed(string filepath)
    {
        using var stream = new FileStream(Assets.GetPath(filepath), FileMode.Open, FileAccess.Read);
        return LoadBinaryUncompressed(stream);
    }

    public static SerializedObjectTree? LoadBinaryUncompressed(Stream stream)
    {
        bool header =
            stream.ReadByte() == (byte)'S' &&
            stream.ReadByte() == (byte)'L' &&
            stream.ReadByte() == (byte)'B' &&
            stream.ReadByte() == (byte)'U';
        if (!header) throw new ArgumentException("The stream did not supply a BinaryUncompressed SerializedObjectTree.");

        int version = ReadInt();
        if (version != _BinaryUncompressedVersion) throw new ArgumentException($"The stream supplied an out of date BinaryUncompressed file. Version: {version}, expected: {_BinaryUncompressedVersion}");

        int typeCount = ReadInt();
        if (typeCount < 1) return null;

        Type?[] types = new Type?[typeCount];
        for (int i = 0; i < typeCount; i++)
        {
            var fullname = ReadString();
            if (fullname != null)
                types[i] = Type.GetType(fullname);
        }

        int serialTypeCount = ReadInt();
        if (serialTypeCount < 1) return null;

        Dictionary<int, SerializedTypeInfo> indexedTypes = new();
        SpanList<SerialHandle> serializedObjects = new();
        SpanList<byte> primitiveData = new();

        for (int i = 0; i < serialTypeCount; i++)
        {
            var type = types[i];
            if (type == null) continue;

            int fieldCount = ReadInt();
            FieldInfo?[] fields = new FieldInfo?[fieldCount];
            for (int f = 0; f < fieldCount; f++)
            {
                int declaringTypeIndex = ReadInt();
                var t = types[declaringTypeIndex];
                System.Console.WriteLine(t?.Name);
                if (declaringTypeIndex == -1) continue;

                var fieldName = ReadString();
                System.Console.WriteLine(fieldName);
                if (fieldName == null) continue;

                fields[f] = t?.GetField(fieldName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }

            int methodCount = ReadInt();
            MethodInfo?[] methods = new MethodInfo?[methodCount];

            indexedTypes.Add(i, new(type, new(fields), new(methods)));
        }

        return null!;

        int ReadInt()
        {
            Span<byte> binaryInt = stackalloc byte[4];
            stream.ReadExactly(binaryInt);
            return BinaryPrimitives.ReadInt32LittleEndian(binaryInt);
        }
        string? ReadString()
        {
            int length = ReadInt();
            System.Console.WriteLine(length);
            if (length < 1) return null;

            StringBuilder builder = new(length, length);
            Span<byte> binaryChar = stackalloc byte[2];
            for (int i = 0; i < length; i++)
            {
                stream.Read(binaryChar);
                builder.Append((char)BinaryPrimitives.ReadInt16LittleEndian(binaryChar));
            }
            return builder.ToString();
        }
    }

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
        using StreamWriter textReader = new(stream, Encoding.Unicode);

        WriteIntToStream(_BinaryUncompressedVersion); // version

        Dictionary<Type, int> allNamedTypes = new();
        int typeCount = _indexedTypes.Count;
        for (int i = 0; i < typeCount; i++)
        {
            var type = _indexedTypes[i];
            allNamedTypes.TryAdd(type.Type, -1);
            foreach (var field in type.Fields)
            {
                if (field?.DeclaringType != null)
                    allNamedTypes.TryAdd(field.DeclaringType, -1);
            }
            foreach (var method in type.OnSerializeMethods)
            {
                if (method?.DeclaringType != null)
                    allNamedTypes.TryAdd(method.DeclaringType, -1);
            }
        }
        var types = allNamedTypes.ToArray();

        int typeIndex = 0;
        WriteIntToStream(types.Length); // the amount of types
        foreach ((var type, var _) in types)
        {
            allNamedTypes[type] = typeIndex;
            if (!TryWriteStringToStream(type.AssemblyQualifiedName)) // every single type's full name
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
                WriteIntToStream(f?.DeclaringType != null ? allNamedTypes[f.DeclaringType] : -1); // declaring type of the field
                TryWriteStringToStream(f?.Name); // name of the field
            }
            WriteIntToStream(type.OnSerializeMethods.Count); // amount of OnSerialize methods
            foreach (var method in type.OnSerializeMethods)
            {
                WriteIntToStream(method?.DeclaringType != null ? allNamedTypes[method.DeclaringType] : -1); // declaring type of the method
                TryWriteStringToStream(method?.Name); // name of the method
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
            textReader.Write(value);
            textReader.Flush();
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