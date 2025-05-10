using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using SlopperEngine.Core.Collections;

namespace SlopperEngine.SceneObjects.Serialization;

/// <summary>
/// The serialized form of a SceneObject tree.
/// </summary>
public partial class SerializedObjectTree
{
    Dictionary<int, (Type, ReadOnlyCollection<FieldInfo?> fields, ReadOnlyCollection<MethodInfo?> onSerializeMethods)> _indexedTypes = new();
    SpanList<SerialHandle> _serializedObjects = new(); // warning: this is 1 indexed! 0 is for null and should NEVER be deserialized.
    SpanList<byte> _primitiveData = new();

    public SceneObject Instantiate()
    {
        Dictionary<int, object?> deserializedObjects = new();
        return (SceneObject)RecursiveDeserialize(1, deserializedObjects)!;
    }
    object? RecursiveDeserialize(int serialHandleIndex, Dictionary<int, object?> deserializedObjects)
    {
        SerialHandle thisObject = _serializedObjects[serialHandleIndex];
        if(thisObject.SaveFields)
        {
            if(thisObject.Handle == 0)
            {
                // nullref - dont set anything
                System.Console.WriteLine("field nullref");
                return null;
            }
            (Type t, ReadOnlyCollection<FieldInfo?> fields, ReadOnlyCollection<MethodInfo?> onSerializeMethods) = _indexedTypes[thisObject.IndexedType];
            
            object res;
            try{res = RuntimeHelpers.GetUninitializedObject(t);}
            catch(ArgumentException){return null;}

            deserializedObjects[serialHandleIndex] = res;
            int currentField = thisObject.Handle - 1;
            foreach(var field in fields)
            {
                currentField++;
                if(field is null)
                    continue;

                if(currentField == 37)
                {
                    //. lol
                }
                
                object? fieldValue = RecursiveDeserialize(currentField, deserializedObjects);
                
                if(fieldValue != null)
                    field.SetValue(res, fieldValue);
            }
            return res;
        }
        else 
        switch(thisObject.SerialType)
        {
            case SerialHandle.Type.Primitive:
            return ReadPrimitive(thisObject);

            case SerialHandle.Type.Reference:
            if(_indexedTypes[thisObject.IndexedType].Item1 == typeof(string))
                return ReadString(thisObject);
            return null;

            case SerialHandle.Type.ReferenceToPrevious:
            if(deserializedObjects.TryGetValue(thisObject.Handle, out var res))
            {
                // le debuggerinatoration
                return res;
            }
            return null;

            case SerialHandle.Type.Array:
            return ReadArray(thisObject, deserializedObjects);

            default:
            return null;
        }
    }

    Array? ReadArray(SerialHandle handle, Dictionary<int, object?> deserializedObjects)
    {
        SerialHandle rank = _serializedObjects[handle.Handle];
        var type = _indexedTypes[rank.IndexedType].Item1;
        if(rank.Handle == 1)
        {
            SerialHandle length = _serializedObjects[handle.Handle + 1];
            Array res = Array.CreateInstance(type, length.Handle);
            for(int i = 0; i<res.Length; i++)
            {
                var val = RecursiveDeserialize(handle.Handle+2, deserializedObjects);
                res.SetValue(val, i);
            }
            return res;
        }
        throw new ArgumentException("Deserializing multi-dimensional arrays isnt supported yet.");
    }

    string? ReadString(SerialHandle handle)
    {
        int count = BinaryPrimitives.ReadInt32LittleEndian(_primitiveData.AllValues.Slice(handle.Handle));
        return Encoding.Unicode.GetString(_primitiveData.AllValues.Slice(handle.Handle + 4, count));
    }

    object? ReadPrimitive(SerialHandle handle)
    {
        Type t = _indexedTypes[handle.IndexedType].Item1;
        int primitiveSize = Marshal.SizeOf(t);
        if(t == typeof(bool)) return _primitiveData.AllValues[handle.Handle] != 0;
        var span = _primitiveData.AllValues.Slice(handle.Handle, primitiveSize);

        if(t == typeof(float)) return BinaryPrimitives.ReadSingleLittleEndian(span);
        if(t == typeof(double)) return BinaryPrimitives.ReadDoubleLittleEndian(span);

        if(t == typeof(int)) return BinaryPrimitives.ReadInt32LittleEndian(span);
        if(t == typeof(uint)) return BinaryPrimitives.ReadUInt32LittleEndian(span);

        if(t == typeof(char)) return ReadIntLittleEndian<char>(span);
        
        if(t == typeof(byte)) return span[0];
        if(t == typeof(short)) return BinaryPrimitives.ReadInt16LittleEndian(span);

        if(t == typeof(long)) return BinaryPrimitives.ReadUInt32LittleEndian(span);
        if(t == typeof(ulong)) return BinaryPrimitives.ReadUInt32LittleEndian(span);

        if(t == typeof(sbyte)) return (sbyte)span[0];
        if(t == typeof(ushort)) return BinaryPrimitives.ReadUInt16LittleEndian(span);

        if(t == typeof(nint)) return (nint)BinaryPrimitives.ReadInt64LittleEndian(span);
        if(t == typeof(nuint)) return (nuint)BinaryPrimitives.ReadUInt64LittleEndian(span);

        return null;//throw new Exception($"Couldnt deserialize type {t.Name}");

        T ReadIntLittleEndian<T>(ReadOnlySpan<byte> span) where T : IBinaryInteger<T> => T.ReadLittleEndian(span, true);
    }

    record struct SerialHandle
    {
        public int Handle;
        public int IndexedType;
        public string? DebugTypeName;
        public Type SerialType;
        public bool SaveFields;

        public enum Type
        {
            Reference, 
            ReferenceToPrevious,
            Primitive, 
            Array,
            ArrayCount,
            CustomSerializedObjects,
            CustomSerializedObjectsCount,
        }
    }
}