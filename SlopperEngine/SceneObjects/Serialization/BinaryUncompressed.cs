using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using SlopperEngine.Core;
using SlopperEngine.Core.Collections;
using SlopperEngine.Core.Serialization;

namespace SlopperEngine.SceneObjects.Serialization;

public partial class SerializedObject
{
    const int _BinaryUncompressedVersion = 1;
    public static SerializedObject? LoadBinaryUncompressed(string filepath)
    {
        using var stream = new FileStream(Assets.GetPath(filepath), FileMode.Open, FileAccess.Read);
        return LoadBinaryUncompressed(stream);
    }

    public static SerializedObject? LoadBinaryUncompressed(Stream stream)
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

        Dictionary<int, Type?> allNamedTypes = new();
        for (int i = 0; i < typeCount; i++)
        {
            int index = ReadInt();
            var fullname = ReadString();
            if (fullname != null)
                allNamedTypes[index] = Type.GetType(fullname);
        }

        int serialTypeCount = ReadInt();
        if (serialTypeCount < 1) return null;

        Dictionary<int, SerializedTypeInfo> indexedTypes = new();

        for (int i = 0; i < serialTypeCount; i++)
        {
            var type = allNamedTypes[i];
            if (type == null) continue;

            int fieldCount = ReadInt();
            FieldInfo?[] fields = new FieldInfo?[fieldCount];
            for (int f = 0; f < fieldCount; f++)
            {
                int declaringTypeIndex = ReadInt();
                var t = allNamedTypes[declaringTypeIndex];
                if (declaringTypeIndex == -1)
                {
                    Console.WriteLine("Missing type:" + ReadString());
                    continue;
                }

                var fieldName = ReadString();
                if (fieldName == null) continue;

                fields[f] = t?.GetField(fieldName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }

            int methodCount = ReadInt();
            MethodInfo?[] methods = new MethodInfo?[methodCount];
            for (int m = 0; m < methodCount; m++)
            {
                int declaringTypeIndex = ReadInt();
                var t = allNamedTypes[declaringTypeIndex];
                if (declaringTypeIndex == -1)
                {
                    Console.WriteLine("Missing type:" + ReadString());
                    continue;
                }

                var methodName = ReadString();
                if (methodName == null) continue;

                methods[m] = t?.GetMethod(methodName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }

            indexedTypes.Add(i, new(type, new(fields), new(methods)));
        }

        int serializedObjectCount = ReadInt();
        if (serializedObjectCount < 1) return null;

        SpanList<SerialHandle> serializedObjects = new(serializedObjectCount);
        for (int i = 0; i < serializedObjectCount; i++)
            serializedObjects.Add(ReadSerialHandle());

        int primitiveDataCount = ReadInt();
        SpanList<byte>? primitiveData = default;
        if (primitiveDataCount > 0)
        {
            primitiveData = new(primitiveDataCount);
            primitiveData.AddMultiple(primitiveDataCount);
            stream.Read(primitiveData.AllValues);
        }

        return new(indexedTypes, serializedObjects, primitiveData ?? new());

        int ReadInt()
        {
            Span<byte> binaryInt = stackalloc byte[4];
            stream.ReadExactly(binaryInt);
            return BinaryPrimitives.ReadInt32LittleEndian(binaryInt);
        }
        string? ReadString()
        {
            int length = ReadInt();
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
        SerialHandle ReadSerialHandle()
        {
            SerialHandle res = default;
            res.Handle = ReadInt();
            res.IndexedType = ReadInt();
            res.SerialType = (SerialHandle.Type)stream.ReadByte();
            res.SaveFields = stream.ReadByte() == 1 ? true : false;
            return res;
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
        foreach (var type in _indexedTypes)
            allNamedTypes.Add(type.Value.Type, allNamedTypes.Count);

        for (int i = 0; i < _indexedTypes.Count; i++)
        {
            var type = _indexedTypes[i];
            foreach (var field in type.Fields)
            {
                if (field?.DeclaringType != null)
                    allNamedTypes.TryAdd(field.DeclaringType, allNamedTypes.Count);
            }
            foreach (var method in type.OnSerializeMethods)
            {
                if (method?.DeclaringType != null)
                    allNamedTypes.TryAdd(method.DeclaringType, allNamedTypes.Count);
            }
        }

        int typeIndex = 0;
        WriteIntToStream(allNamedTypes.Count); // the amount of types
        foreach (var namedType in allNamedTypes)
        {
            WriteIntToStream(namedType.Value);
            if (!TryWriteStringToStream(namedType.Key.AssemblyQualifiedName)) // every single type's full name
                System.Console.WriteLine($"Type {namedType.Key} couldnt be written to a string.");

            typeIndex++;
        }

        WriteIntToStream(_indexedTypes.Count); // amount of serialized types

        for (int i = 0; i < _indexedTypes.Count; i++)
        {
            var type = _indexedTypes[i];
            var index = allNamedTypes[type.Type];
            WriteIntToStream(type.Fields.Count); // amount of fields
            foreach (var f in type.Fields)
            {
                int fieldDeclaringTypeIndex = f?.DeclaringType != null ? allNamedTypes[f.DeclaringType] : -1;
                WriteIntToStream(fieldDeclaringTypeIndex); // declaring type of the field
                TryWriteStringToStream(f?.Name); // name of the field
            }

            WriteIntToStream(type.OnSerializeMethods.Count); // amount of OnSerialize methods
            foreach (var method in type.OnSerializeMethods)
            {
                int methodDeclaringTypeIndex = method?.DeclaringType != null ? allNamedTypes[method.DeclaringType] : -1;
                WriteIntToStream(methodDeclaringTypeIndex); // declaring type of the method
                TryWriteStringToStream(method?.Name); // name of the method
            }
        }

        WriteIntToStream(_serializedObjects.Count); // amount of serialHandles

        foreach (var h in _serializedObjects.AllValues)
            WriteSerialHandleToStream(h);

        WriteIntToStream(_primitiveData.Count); // amount of primitive data
        stream.Write(_primitiveData.AllValues);

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
        void WriteSerialHandleToStream(SerialHandle value)
        {
            Span<byte> asBytes = stackalloc byte[4 + 4 + 1 + 1];
            BinaryPrimitives.WriteInt32LittleEndian(asBytes, value.Handle);
            BinaryPrimitives.WriteInt32LittleEndian(asBytes.Slice(4), value.IndexedType);
            asBytes[8] = (byte)value.SerialType;
            asBytes[9] = value.SaveFields ? (byte)1 : (byte)0;
            stream.Write(asBytes);
        }
    }
}