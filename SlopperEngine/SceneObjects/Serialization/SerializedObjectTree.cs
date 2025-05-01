using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using SlopperEngine.Core;
using SlopperEngine.Core.Collections;

namespace SlopperEngine.SceneObjects.Serialization;

/// <summary>
/// The serialized form of a SceneObject tree.
/// </summary>
public class SerializedObjectTree
{
    int _typeCount = 1;
    Dictionary<Type, int> _typeIndices = new();
    Dictionary<int, Type> _indexTypes = new();
    int _currentID = 1;
    Dictionary<object, int> _referenceIDs = new();
    SpanList<byte> _primitiveData = new();

    /// <summary>
    /// Serializes a SceneObject. Should only be called by SceneObject.Serialize().
    /// </summary>
    internal SerializedObjectTree(SceneObject toSerialize)
    {
        if(toSerialize.InScene) 
            throw new Exception("SceneObject was in the scene while being serialized - call SceneObject.Serialize() to properly serialize it.");
        /*_objectType = toSerialize.GetType();
        _fields = SceneObjectReflectionCache.GetSerializableFields(_objectType);
        _fieldValues = new object[_fields.Count];
        for(int i = 0; i<_fields.Count; i++)
        {
            _fieldValues[i] = _fields[i].GetValue(toSerialize);
            System.Console.WriteLine($"{_fields[i].Name}, {_fieldValues[i]}");
        }*/
    }

    int GetTypeIndex(Type t)
    {
        if(_typeIndices.TryGetValue(t, out int res)) 
            return res;
        _typeIndices[t] = _typeCount;
        _typeCount++;
        return _typeCount-1;
    }

    SerialHandle AddObject(object obj)
    {
        var type = obj.GetType();
        if(type.IsPrimitive)
            return WritePrimitive(obj);

        SerialHandle res = default;
        res.IsPrimitive = false;
        if(_referenceIDs.TryGetValue(obj, out res.Handle))
            return res;
        
        res.Handle = _currentID;
        _referenceIDs[obj] = _currentID;
        _currentID++;

        return res;
    }
    
    SerialHandle WritePrimitive(object obj)
    {
        SerialHandle res = default;
        res.IsPrimitive = true;
        int headerSize = Unsafe.SizeOf<PrimitiveHeader>();

        // if statements sorted by how common i think the type is
        if(obj is float f) {WriteFloat(f); return res;}
        if(obj is double d) {WriteFloat(d); return res;}

        if(obj is int i) {WriteInt(i); return res;}
        if(obj is uint ui) {WriteInt(ui); return res;}

        if(obj is bool b)
        {
            res.Handle = _primitiveData.Count;
            PrimitiveHeader header = default;
            header.Size = 1;
            header.IndexedType = GetTypeIndex(typeof(bool));

            var span = _primitiveData.Add(headerSize + header.Size);

            header.Write(span);
            span[headerSize] = b ? (byte)1 : (byte)0;
            return res;
        }

        if(obj is char c) {WriteInt(c); return res;}

        if(obj is byte by) {WriteInt(by); return res;}
        if(obj is short s) {WriteInt(s); return res;}

        if(obj is long l) {WriteInt(l); return res;}
        if(obj is ulong ul) {WriteInt(ul); return res;}

        if(obj is sbyte sb) {WriteInt(sb); return res;}
        if(obj is ushort us) {WriteInt(us); return res;}
        
        return res; // im not serializing decimal ig

        void WriteInt<T>(T num) where T : unmanaged, IBinaryInteger<T>
        {
            res.Handle = _primitiveData.Count;
            PrimitiveHeader header = default;
            header.Size = Unsafe.SizeOf<T>();
            header.IndexedType = GetTypeIndex(typeof(T));

            var span = _primitiveData.Add(headerSize + header.Size);
            
            header.Write(span);
            num.TryWriteLittleEndian(span.Slice(headerSize), out _);
        }
        unsafe void WriteFloat<T>(T num) where T : unmanaged, IBinaryFloatingPointIeee754<T>
        {
            res.Handle = _primitiveData.Count;
            PrimitiveHeader header = default;
            header.Size = Unsafe.SizeOf<T>();
            header.IndexedType = GetTypeIndex(typeof(T));

            var span = _primitiveData.Add(headerSize + header.Size);

            header.Write(span);
            if(header.Size == 8)
            {
                long value = *(long*)&num;
                (value as IBinaryInteger<long>).TryWriteLittleEndian(span.Slice(headerSize), out _);
            }
            else if(header.Size == 4)
            {
                int value = *(int*)&num;
                (value as IBinaryInteger<int>).TryWriteLittleEndian(span.Slice(headerSize), out _);
            }
            else throw new Exception("What kinda fucking float is that");
        }
    }

    object ReadPrimitive(SerialHandle handle)
    {
        int headerSize = Unsafe.SizeOf<PrimitiveHeader>();
        PrimitiveHeader header = default;
        header.Read(_primitiveData.AllValues.Slice(handle.Handle, headerSize));
        Type t = _indexTypes[header.IndexedType];
        var span = _primitiveData.AllValues.Slice(handle.Handle + headerSize, header.Size);

        if(t == typeof(float)) return BinaryPrimitives.ReadSingleLittleEndian(span);
        if(t == typeof(double)) return BinaryPrimitives.ReadDoubleLittleEndian(span);

        if(t == typeof(int)) return BinaryPrimitives.ReadInt32LittleEndian(span);
        if(t == typeof(uint)) return BinaryPrimitives.ReadUInt32LittleEndian(span);

        if(t == typeof(bool)) return span[0] != 0;
        if(t == typeof(char)) return ReadIntLittleEndian<char>(span);
        
        if(t == typeof(byte)) return span[0];
        if(t == typeof(short)) return BinaryPrimitives.ReadInt16LittleEndian(span);

        if(t == typeof(long)) return BinaryPrimitives.ReadUInt32LittleEndian(span);
        if(t == typeof(ulong)) return BinaryPrimitives.ReadUInt32LittleEndian(span);

        if(t == typeof(sbyte)) return (sbyte)span[0];
        if(t == typeof(ushort)) return BinaryPrimitives.ReadUInt16LittleEndian(span);

        throw new Exception($"Couldnt deserialize type {t.Name}");

        T ReadIntLittleEndian<T>(ReadOnlySpan<byte> span) where T : IBinaryInteger<T> => T.ReadLittleEndian(span, true);
    }

    /// <summary>
    /// Serializes objects.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="writes"></param>
    public struct Serializer(SerializedObjectTree target, bool writes)
    {
        /// <summary>
        /// True if this Serializer writes to given values.
        /// </summary>
        public bool IsWriter => _writes;

        /// <summary>
        /// True if this Serializer reads from given values.
        /// </summary>
        public bool IsReader => !_writes;
        
        SerializedObjectTree _target = target;
        bool _writes = writes;

        /// <summary>
        /// Serializes a given value.
        /// </summary>
        /// <param name="Value">The value to serialize.</param>
        public void Serialize(ref object Value)
        {

        }
    }

    struct PrimitiveHeader
    {
        public int Size;
        public int IndexedType;

        public void Write(Span<byte> res)
        {
            (Size as IBinaryInteger<int>).WriteLittleEndian(res);
            (IndexedType as IBinaryInteger<int>).WriteLittleEndian(res.Slice(4));
        }

        public void Read(Span<byte> input)
        {
            Size = BinaryPrimitives.ReadInt32LittleEndian(input);
            IndexedType = BinaryPrimitives.ReadInt32LittleEndian(input.Slice(4));
        }
    }
    struct SerialHandle
    {
        public int Handle;
        public bool IsPrimitive;
    }
}