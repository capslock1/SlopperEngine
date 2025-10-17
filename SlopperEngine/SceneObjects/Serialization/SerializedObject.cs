using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using SlopperEngine.Core.Collections;
using SlopperEngine.Core.Serialization;

namespace SlopperEngine.SceneObjects.Serialization;

/// <summary>
/// The serialized form of a SceneObject tree.
/// </summary>
public partial class SerializedObject
{
    Dictionary<int, SerializedTypeInfo> _indexedTypes = new(); // just realized this is just a list? the index is from 0..count... im not fixing this rn but what the hell
    SpanList<SerialHandle> _serializedObjects = new(); // warning: this is 1 indexed! 0 is for null and should NEVER be deserialized.
    SpanList<byte> _primitiveData = new();
    event Action? _onFinishSerializing;

    private SerializedObject(Dictionary<int, SerializedTypeInfo> indexedTypes, SpanList<SerialHandle> serializedObjects, SpanList<byte> primitiveData)
    {
        _indexedTypes = indexedTypes;
        _serializedObjects = serializedObjects;
        _primitiveData = primitiveData;
    }

    public object Instantiate()
    {
        Dictionary<int, object?> deserializedObjects = new();
        var res = RecursiveDeserialize(1, deserializedObjects)!;
        _onFinishSerializing?.Invoke();
        _onFinishSerializing = null;
        return res;
    }

    /// <summary>
    /// Calls the method after serialization / deserialization finishes. It's safe to interact with objects higher in the tree after this.
    /// </summary>
    /// <param name="methodToCall">The method to have called after the entire tree is serialized.</param>
    public void CallAfterSerialize(Action methodToCall)
    {
        _onFinishSerializing += methodToCall;
    }

    object? RecursiveDeserialize(int serialHandleIndex, Dictionary<int, object?> deserializedObjects)
    {
        SerialHandle thisObject = _serializedObjects[serialHandleIndex];
        if (thisObject.SaveFields)
        {
            if (thisObject.Handle == 0)
                // nullref - dont set anything
                return null;

            var t = _indexedTypes[thisObject.IndexedType];

            object res;
            try { res = RuntimeHelpers.GetUninitializedObject(t.Type); }
            catch (ArgumentException) { return null; }

            deserializedObjects[serialHandleIndex] = res;
            int currentField = thisObject.Handle - 1;
            foreach (var field in t.Fields)
            {
                currentField++;
                if (field is null)
                    continue;

                object? fieldValue = RecursiveDeserialize(currentField, deserializedObjects);

                if (fieldValue != null)
                    field.SetValue(res, fieldValue);
            }
            foreach (var method in t.OnSerializeMethods)
            {
                currentField++;
                if (method is null)
                    continue;

                SerialHandle methodP = _serializedObjects[currentField];
                SerialHandle serialCount = _serializedObjects[methodP.Handle];
                int refint = 0;
                OnSerializeArgs serializer = new(methodP.Handle + 1, serialCount.Handle, deserializedObjects, this, ref refint, new(this));
                CallOnSerializeQuick(method, res, serializer);
            }
            return res;
        }
        else
            switch (thisObject.SerialType)
            {
                case SerialHandle.Type.Primitive:
                    return ReadPrimitive(thisObject);

                case SerialHandle.Type.Reference:
                    if (_indexedTypes[thisObject.IndexedType].Type == typeof(string))
                    {
                        var res = ReadString(thisObject);
                        deserializedObjects[serialHandleIndex] = res;
                        return res;
                    }
                    return null;

                case SerialHandle.Type.ReferenceToPrevious:
                    deserializedObjects.TryGetValue(thisObject.Handle, out var res1);
                    return res1;

                case SerialHandle.Type.Array:
                    var res2 = ReadArray(thisObject, deserializedObjects);
                    deserializedObjects[serialHandleIndex] = res2;
                    return res2;

                case SerialHandle.Type.SerializedFromKey:
                    Type thisObjectType = _indexedTypes[thisObject.IndexedType].Type;
                    Type keyType = _indexedTypes[_serializedObjects[thisObject.Handle].IndexedType].Type;
                    object? key = RecursiveDeserialize(thisObject.Handle + 1, deserializedObjects);
                    var res3 = ReflectionCache.DeserializeObjectFromKey(keyType, thisObjectType, key);
                    deserializedObjects[serialHandleIndex] = res3;
                    return res3;

                default:
                    return null;
            }
    }

    Array? ReadArray(SerialHandle handle, Dictionary<int, object?> deserializedObjects)
    {
        SerialHandle rank = _serializedObjects[handle.Handle];
        var type = _indexedTypes[rank.IndexedType].Type;
        if (rank.Handle == 1)
        {
            SerialHandle length = _serializedObjects[handle.Handle + 1];
            Array res = Array.CreateInstance(type, length.Handle);
            for (int i = 0; i < res.Length; i++)
            {
                var val = RecursiveDeserialize(handle.Handle + 2 + i, deserializedObjects);
                res.SetValue(val, i);
            }
            return res;
        }
        throw new ArgumentException("Deserializing multi-dimensional arrays isnt supported yet.");
    }

    string? ReadString(SerialHandle handle)
    {
        int count = BinaryPrimitives.ReadInt32LittleEndian(_primitiveData.AllValues.Slice(handle.Handle));
        return Encoding.UTF8.GetString(_primitiveData.AllValues.Slice(handle.Handle + 4, count));
    }

    object? ReadPrimitive(SerialHandle handle)
    {
        Type t = _indexedTypes[handle.IndexedType].Type;
        int primitiveSize = Marshal.SizeOf(t);
        if (t == typeof(bool)) return _primitiveData.AllValues[handle.Handle] != 0;
        var span = _primitiveData.AllValues.Slice(handle.Handle, primitiveSize);

        if (t == typeof(float)) return BinaryPrimitives.ReadSingleLittleEndian(span);
        if (t == typeof(double)) return BinaryPrimitives.ReadDoubleLittleEndian(span);

        if (t == typeof(int)) return BinaryPrimitives.ReadInt32LittleEndian(span);
        if (t == typeof(uint)) return BinaryPrimitives.ReadUInt32LittleEndian(span);

        if (t == typeof(char)) return ReadIntLittleEndian<char>(span);

        if (t == typeof(byte)) return span[0];
        if (t == typeof(short)) return BinaryPrimitives.ReadInt16LittleEndian(span);

        if (t == typeof(long)) return BinaryPrimitives.ReadUInt32LittleEndian(span);
        if (t == typeof(ulong)) return BinaryPrimitives.ReadUInt32LittleEndian(span);

        if (t == typeof(sbyte)) return (sbyte)span[0];
        if (t == typeof(ushort)) return BinaryPrimitives.ReadUInt16LittleEndian(span);

        if (t == typeof(nint)) return (nint)BinaryPrimitives.ReadInt64LittleEndian(span);
        if (t == typeof(nuint)) return (nuint)BinaryPrimitives.ReadUInt64LittleEndian(span);

        return null;//throw new Exception($"Couldnt deserialize type {t.Name}");

        T ReadIntLittleEndian<T>(ReadOnlySpan<byte> span) where T : IBinaryInteger<T> => T.ReadLittleEndian(span, true);
    }

    /// <summary>
    /// Calls the OnSerialize method quickly, if the object can actually have it called. Or if it can't - this is VERY UNSAFE!!!
    /// </summary>
    static unsafe void CallOnSerializeQuick(MethodInfo OnSerializeMethod, object target, OnSerializeArgs serializer) =>
        ((delegate*<object, OnSerializeArgs, void>)OnSerializeMethod.MethodHandle.GetFunctionPointer())
            (target, serializer);

    public record struct SerializationToken(SerializedObject owner)
    {
        public object? RecursiveDeserialize(int serialHandleIndex, Dictionary<int, object?> deserializedObjects)
        {
            return owner.RecursiveDeserialize(serialHandleIndex, deserializedObjects);
        }
    }
}