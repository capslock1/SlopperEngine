using System.Buffers.Binary;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
    Dictionary<int, (Type, ReadOnlyCollection<FieldInfo?>)> _indexTypes = new();
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
        SerializationRefs refs = new();
        refs.ReferenceIDs = new();
        refs.TypeIndices = new();
        res[0] = SerializeRecursive(toSerialize, refs);

        #pragma warning disable CS8620
        foreach(var comb in refs.TypeIndices)
            _indexTypes.Add(comb.Value.Item1, (comb.Key, comb.Value.Item2));
        #pragma warning restore CS8620
    }

    public void WriteOutTree()
    {
        (Type root, ReadOnlyCollection<FieldInfo?> fields) = _indexTypes[_rootTypeIndex];

        TextWriter w = new StringWriter();
        IndentedTextWriter writer = new(w);
        //try{
            RecursiveWriteTree(0, root, fields, writer);
        //}catch(Exception e){ System.Console.WriteLine(e.Message);}
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

            if(handle.SaveFields)
            {
                output.WriteLine();
                if(handle.Handle == 0)
                    continue;

                (Type fieldT, ReadOnlyCollection<FieldInfo?> infos) = _indexTypes[handle.IndexedType];
                RecursiveWriteTree(handle.Handle-1, fieldT, infos, output);
            }
            else if(!handle.SaveReference)
            {
                output.Write(" (primitive): ");
                output.WriteLine(ReadPrimitive(handle) ?? "null");
            }
            else output.WriteLine(" (reference up the tree).");
        }
        output.Indent--;
    }

    SerialHandle SerializeRecursive(object toSerialize, in SerializationRefs refs)
    {
        var type = toSerialize.GetType();
        int tIndex = GetTypeIndex(type, refs);
        if(_rootTypeIndex == -1)
            _rootTypeIndex = tIndex;

        var handle = AddObject(toSerialize, refs, out bool isNewReference);
        handle.IndexedType = tIndex;

        if(!handle.SaveFields || !isNewReference)
            return handle;

        handle.Handle = _serializedObjects.Count;
        var fields = SceneObjectReflectionCache.GetSerializableFields(type);
        var fieldSpan = _serializedObjects.Add(fields.Count);

        for(int i = 0; i<fieldSpan.Length; i++)
        {
            var fieldVal = fields[i].GetValue(toSerialize);
            if(fieldVal != null)
            {
                var serialFieldVal = SerializeRecursive(fieldVal, refs);
                fieldSpan[i] = serialFieldVal;
            }
        }

        return handle;
    }

    int GetTypeIndex(Type t, in SerializationRefs refs)
    {
        if(refs.TypeIndices.TryGetValue(t, out var res)) 
            return res.Item1;
        #pragma warning disable CS8619
        res = refs.TypeIndices[t] = (refs.TypeIndices.Count, SceneObjectReflectionCache.GetSerializableFields(t));
        #pragma warning restore CS8619
        return res.Item1;
    }

    SerialHandle AddObject(object obj, in SerializationRefs refs, out bool newReference)
    {
        newReference = false;
        var type = obj.GetType();
        if(type.IsPrimitive)
            return WritePrimitive(obj, refs);
        
        if(type == typeof(string))
            return SerializeString((string)obj, refs);

        SerialHandle res = default;
        res.SaveReference = true;
        if(refs.ReferenceIDs.TryGetValue(obj, out res.Handle))
            return res;

        res.SaveFields = true;
        newReference = true;
        refs.ReferenceIDs.Add(obj, _serializedObjects.Count);

        return res;
    }

    SerialHandle SerializeString(string obj, in SerializationRefs refs)
    {
        SerialHandle res = default;
        res.SaveReference = true;
        res.SaveFields = false;
        res.Handle = _primitiveData.Count;

        int stringLength = Encoding.Unicode.GetByteCount(obj);
        res.IndexedType = GetTypeIndex(typeof(string), refs);

        var span = _primitiveData.Add(Encoding.Unicode.GetByteCount(obj) + 4);
        BinaryPrimitives.WriteInt32LittleEndian(span, stringLength);
        Encoding.Unicode.GetBytes(obj, span.Slice(4));

        return res;
    }
    
    SerialHandle WritePrimitive(object obj, in SerializationRefs refs)
    {
        SerialHandle res = default;
        res.SaveReference = false;
        res.SaveFields = false;
        res.Handle = _primitiveData.Count;
        int size = Marshal.SizeOf(obj.GetType());

        // if statements sorted by how common i think the type is
        if(obj is float f) {WriteFloat(f, refs); return res;}
        if(obj is double d) {WriteFloat(d, refs); return res;}

        if(obj is int i) {WriteInt(i, refs); return res;}
        if(obj is uint ui) {WriteInt(ui, refs); return res;}

        if(obj is bool b)
        {
            res.IndexedType = GetTypeIndex(typeof(bool), refs);
            _primitiveData.Add(b ? (byte)1 : (byte)0);
            return res;
        }

        if(obj is char c) {WriteInt(c, refs); return res;}

        if(obj is byte by) {WriteInt(by, refs); return res;}
        if(obj is short s) {WriteInt(s, refs); return res;}

        if(obj is long l) {WriteInt(l, refs); return res;}
        if(obj is ulong ul) {WriteInt(ul, refs); return res;}

        if(obj is sbyte sb) {WriteInt(sb, refs); return res;}
        if(obj is ushort us) {WriteInt(us, refs); return res;}

        if(obj is nint ni) {WriteInt(ni, refs, 8); return res;}
        if(obj is nuint nu) {WriteInt(nu, refs, 8); return res;}
        
        return res; // im not serializing decimal ig

        void WriteInt<T>(T num, in SerializationRefs refs, int overrideSizeof = -1) where T : unmanaged, IBinaryInteger<T>
        {
            int ssize = overrideSizeof < 0 ? Unsafe.SizeOf<T>() : overrideSizeof;
            res.IndexedType = GetTypeIndex(typeof(T), refs);

            var span = _primitiveData.Add(ssize);
            num.TryWriteLittleEndian(span, out _);
        }
        unsafe void WriteFloat<T>(T num, in SerializationRefs refs) where T : unmanaged, IBinaryFloatingPointIeee754<T>
        {
            int ssize = Unsafe.SizeOf<T>();
            res.IndexedType = GetTypeIndex(typeof(T), refs);

            var span = _primitiveData.Add(ssize);

            if(ssize == 8)
            {
                long value = *(long*)&num;
                (value as IBinaryInteger<long>).TryWriteLittleEndian(span, out _);
            }
            else if(ssize == 4)
            {
                int value = *(int*)&num;
                (value as IBinaryInteger<int>).TryWriteLittleEndian(span, out _);
            }
            else throw new Exception("What kinda fucking float is that");
        }
    }

    object? ReadPrimitive(SerialHandle handle)
    {
        Type t = _indexTypes[handle.IndexedType].Item1;
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

    record struct SerialHandle
    {
        public int Handle;
        public int IndexedType;
        public bool SaveReference;
        public bool SaveFields;
    }
    ref struct SerializationRefs
    {
        public Dictionary<object, int> ReferenceIDs;
        public Dictionary<Type, (int, ReadOnlyCollection<FieldInfo>)> TypeIndices;
    }
}