using System.Buffers.Binary;
using System.CodeDom.Compiler;
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
    int _rootTypeIndex = -1;
    Dictionary<Type, (int, ReadOnlyCollection<FieldInfo?>)> _typeIndices = new();
    Dictionary<int, (Type, ReadOnlyCollection<FieldInfo?>)> _indexTypes = new();
    Dictionary<object, int> _referenceIDs = new();
    SpanList<SerialHandle> _serializedObjects = new();
    SpanList<byte> _primitiveData = new();

    /// <summary>
    /// Serializes a SceneObject. Should only be called by SceneObject.Serialize().
    /// </summary>
    internal SerializedObjectTree(SceneObject toSerialize)
    {
        if(toSerialize.InScene) 
            throw new Exception("SceneObject was in the scene while being serialized - call SceneObject.Serialize() to properly serialize it.");
        
        var res = _serializedObjects.Add(1);
        res[0] = SerializeRecursive(toSerialize);
    }

    public void WriteOutTree()
    {
        foreach(var typeIndex in _typeIndices)
            _indexTypes[typeIndex.Value.Item1] = (typeIndex.Key, typeIndex.Value.Item2);
                
        (Type root, ReadOnlyCollection<FieldInfo?> fields) = _indexTypes[_rootTypeIndex];

        TextWriter w = new StringWriter();
        IndentedTextWriter writer = new(w);
        RecursiveWriteTree(0, root, fields, writer);
        System.Console.WriteLine(w.ToString());
    }

    void RecursiveWriteTree(int serialHandleIndex, Type t, ReadOnlyCollection<FieldInfo?> fields, IndentedTextWriter output)
    {
        int currentField = serialHandleIndex;
        output.WriteLine(t.Name);
        output.Indent++;
        foreach(var field in fields)
        {
            currentField++;
            if(field is null)
                continue;
            
            output.Write(field.Name);
            SerialHandle handle = _serializedObjects[currentField];

            if(handle.SaveReference)
            {
                output.WriteLine();
                if(handle.Handle == 0)
                    continue;

                Type fieldT = field.FieldType;
                (int tIndex, ReadOnlyCollection<FieldInfo?> infos) = _typeIndices[fieldT];
                RecursiveWriteTree(handle.Handle, fieldT, infos, output);
            }
            else 
            {
                output.WriteLine(ReadPrimitive(handle));
            }
        }
        output.Indent--;
    }

    SerialHandle SerializeRecursive(object toSerialize)
    {
        var type = toSerialize.GetType();
        int tIndex = GetTypeIndex(type);
        if(_rootTypeIndex == -1)
            _rootTypeIndex = tIndex;

        var handle = AddObject(toSerialize, out bool serial);
        if(!handle.SaveReference || !serial)
            return handle;

        handle.Handle = _serializedObjects.Count;
        var fields = SceneObjectReflectionCache.GetSerializableFields(type);
        var fieldSpan = _serializedObjects.Add(fields.Count);
        for(int i = 0; i<fieldSpan.Length; i++)
        {
            var fieldVal = fields[i].GetValue(toSerialize);
            if(fieldVal != null)
                fieldSpan[i] = SerializeRecursive(fieldVal);
        }

        return handle;
    }

    int GetTypeIndex(Type t)
    {
        if(_typeIndices.TryGetValue(t, out var res)) 
            return res.Item1;
        #pragma warning disable CS8619
        res = _typeIndices[t] = (_typeIndices.Count, SceneObjectReflectionCache.GetSerializableFields(t));
        #pragma warning restore CS8619
        return res.Item1;
    }

    SerialHandle AddObject(object obj, out bool newReference)
    {
        newReference = false;
        var type = obj.GetType();
        if(type.IsPrimitive)
            return WritePrimitive(obj);
        
        if(type == typeof(string))
            return SerializeString((string)obj);

        SerialHandle res = default;
        res.SaveReference = true;
        if(_referenceIDs.TryGetValue(obj, out res.Handle))
            return res;

        newReference = true;
        _referenceIDs.Add(obj, _serializedObjects.Count);

        return res;
    }

    SerialHandle SerializeString(string obj)
    {
        SerialHandle res = default;
        res.SaveReference = true;
        res.Handle = _primitiveData.Count;
        int headerSize = Unsafe.SizeOf<PrimitiveHeader>();

        PrimitiveHeader header = default;
        header.Size = Encoding.Unicode.GetByteCount(obj);
        header.IndexedType = GetTypeIndex(typeof(string));

        var span = _primitiveData.Add(headerSize + header.Size);
        header.Write(span);
        Encoding.Unicode.GetBytes(obj, span.Slice(headerSize));

        return res;
    }
    
    SerialHandle WritePrimitive(object obj)
    {
        SerialHandle res = default;
        res.SaveReference = false;
        res.Handle = _primitiveData.Count;
        int headerSize = Unsafe.SizeOf<PrimitiveHeader>();

        // if statements sorted by how common i think the type is
        if(obj is float f) {WriteFloat(f); return res;}
        if(obj is double d) {WriteFloat(d); return res;}

        if(obj is int i) {WriteInt(i); return res;}
        if(obj is uint ui) {WriteInt(ui); return res;}

        if(obj is bool b)
        {
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

        if(obj is nint ni) {WriteInt(ni, 8); return res;}
        if(obj is nuint nu) {WriteInt(nu, 8); return res;}
        
        return res; // im not serializing decimal ig

        void WriteInt<T>(T num, int overrideSizeof = -1) where T : unmanaged, IBinaryInteger<T>
        {
            PrimitiveHeader header = default;

            header.Size = overrideSizeof < 0 ? Unsafe.SizeOf<T>() : overrideSizeof;
            header.IndexedType = GetTypeIndex(typeof(T));

            var span = _primitiveData.Add(headerSize + header.Size);
            
            header.Write(span);
            num.TryWriteLittleEndian(span.Slice(headerSize), out _);
        }
        unsafe void WriteFloat<T>(T num) where T : unmanaged, IBinaryFloatingPointIeee754<T>
        {
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
        Type t = _indexTypes[header.IndexedType].Item1;
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

        if(t == typeof(nint)) return (nint)BinaryPrimitives.ReadInt64LittleEndian(span);
        if(t == typeof(nuint)) return (nuint)BinaryPrimitives.ReadUInt64LittleEndian(span);

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

    record struct PrimitiveHeader
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
    record struct SerialHandle
    {
        public int Handle;
        public bool SaveReference;
    }
}