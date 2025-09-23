using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using SlopperEngine.Core.Serialization;

namespace SlopperEngine.SceneObjects.Serialization;

public partial class SerializedObject
{
    /// <summary>
    /// Serializes a SceneObject. Should only be called by SceneObject.Serialize().
    /// </summary>
    public SerializedObject(SceneObject toSerialize)
    {
        bool insc = toSerialize.InScene;
        var sc = toSerialize.Scene;
        if(insc)
        {
            toSerialize.Unregister();
            sc!.UpdateRegister();
        }

        var res = _serializedObjects.AddMultiple(2);
        SerializationRefs refs = new();
        refs.ReferenceIDs = new();
        refs.TypeIndices = new();
        var rootHandle = SerializeRecursive(toSerialize, 1, refs);
        res[1] = rootHandle;

#pragma warning disable CS8620
        foreach (var comb in refs.TypeIndices)
            _indexedTypes.Add(comb.Value.Item1, new(comb.Key, comb.Value.Item2, comb.Value.Item3));
#pragma warning restore CS8620
        
        _onFinishSerializing?.Invoke();
        _onFinishSerializing = null;

        if (insc)
            toSerialize.Register(sc!);
    }

    SerialHandle SerializeRecursive(object toSerialize, int destinationIndex, in SerializationRefs refs)
    {
        var type = toSerialize.GetType();
        int tIndex = GetTypeIndex(type, refs);

        var handle = AddObject(toSerialize, destinationIndex, refs, out bool isNewReference);
        handle.IndexedType = tIndex;
        handle.DebugTypeName = type.Name;

        if(!handle.SaveFields || !isNewReference)
            return handle;

        handle.Handle = _serializedObjects.Count;
        var fields = ReflectionCache.GetSerializableFields(type);
        var methods = ReflectionCache.GetOnSerializeMethods(type);
        var dataSpan = _serializedObjects.AddMultiple(fields.Count + methods.Count);

        int serialIndex = 0;
        // serialize the fields of the object
        for(; serialIndex<fields.Count; serialIndex++)
        {
            var fieldVal = fields[serialIndex].GetValue(toSerialize);
            if(fieldVal != null)
            {
                var serialFieldVal = SerializeRecursive(fieldVal, handle.Handle+serialIndex, refs);
                dataSpan[serialIndex] = serialFieldVal;
            }
        }
        // serialize the OnSerialize methods of the object
        foreach(var meth in methods)
        {
            List<object?>? results = null;
            OnSerializeArgs serializer = new(ref results, this);
            CallOnSerializeQuick(meth, toSerialize, serializer);

            SerialHandle methodResHandle = default;
            methodResHandle.SerialType = SerialHandle.Type.CustomSerializedObjects;
            methodResHandle.Handle = _serializedObjects.Count;
            dataSpan[serialIndex] = methodResHandle;

            SerialHandle methodResCount = default;
            methodResCount.SerialType = SerialHandle.Type.CustomSerializedObjectsCount;
            methodResCount.Handle = results?.Count ?? 0;
            _serializedObjects.Add(methodResCount);

            if(results != null)
            {
                var methodResultSpan = _serializedObjects.AddMultiple(results.Count);
                for(int i = 0; i<results.Count; i++)
                {
                    var obj = results[i];
                    if(obj is null) continue;
                    int tgtIndex = methodResHandle.Handle + i;
                    var value = SerializeRecursive(obj, tgtIndex, refs);
                    methodResultSpan[i] = value;
                }
            }
            serialIndex++;
        }

        return handle;
    }
    
    int GetTypeIndex(Type t, in SerializationRefs refs)
    {
        if(refs.TypeIndices.TryGetValue(t, out var res)) 
            return res.Item1;

        res = refs.TypeIndices[t] = (refs.TypeIndices.Count, ReflectionCache.GetSerializableFields(t), ReflectionCache.GetOnSerializeMethods(t));
        return res.Item1;
    }

    SerialHandle AddObject(object obj, int destinationIndex, in SerializationRefs refs, out bool newReference)
    {
        newReference = false; // dont save a primitive's reference because its probably not worth the lookup time
        var type = obj.GetType();
        if(type.IsPrimitive)
            return WritePrimitive(obj, refs);
        
        SerialHandle res = default;
        if(refs.ReferenceIDs.TryGetValue(obj, out res.Handle))
        {
            res.SerialType = SerialHandle.Type.ReferenceToPrevious;
            return res;
        }

        newReference = true;
        refs.ReferenceIDs.Add(obj, destinationIndex);
        
        if (type == typeof(string))
            return SerializeString((string)obj, refs);

        if(obj is Array arr)
            return SerializeArray(arr, refs);

        if(obj is SceneObject sc && sc.InScene)
        {
            // dont serialize sceneobjects that are in the scene - these are *not* part of the tree, active, and should never *ever* be serialized
            // this is because else, you risk serializing the entire scene, even if you only want to serialize a part
            res.SerialType = SerialHandle.Type.OutsideReference;
            return res;
        }

        if(obj is ISerializableFromKeyBase serFromKey)
        {
            object? objKey = serFromKey.SerializeToObject();
            res.SerialType = SerialHandle.Type.SerializedFromKey;
            res.Handle = _serializedObjects.Count;
            SerialHandle keyType = default;
            keyType.SerialType = SerialHandle.Type.KeyType;
            keyType.IndexedType = GetTypeIndex(serFromKey.GetKeyType(), refs);
            _serializedObjects.Add(keyType);
            _serializedObjects.Add(default);
            var keyHandle = objKey != null ? SerializeRecursive(objKey, res.Handle, refs) : default;
            _serializedObjects[res.Handle+1] = keyHandle;
            return res;
        }
        
        res.SerialType = SerialHandle.Type.Reference;
        res.SaveFields = true;
        return res;
    }
    
    SerialHandle SerializeString(string obj, in SerializationRefs refs)
    {
        SerialHandle res = default;
        res.SerialType = SerialHandle.Type.Reference;
        res.Handle = _primitiveData.Count;

        int stringLength = Encoding.UTF8.GetByteCount(obj);
        res.IndexedType = GetTypeIndex(typeof(string), refs);
        res.DebugTypeName = "string";

        var span = _primitiveData.AddMultiple(Encoding.UTF8.GetByteCount(obj) + 4);
        BinaryPrimitives.WriteInt32LittleEndian(span, stringLength);
        Encoding.UTF8.GetBytes(obj, span.Slice(4));

        return res;
    }

    SerialHandle SerializeArray(Array array, in SerializationRefs refs)
    {
        SerialHandle res = default;
        res.SerialType = SerialHandle.Type.Array;
        res.Handle = _serializedObjects.Count;
        var elementType = array.GetType().GetElementType();
        if (elementType is null)
            return default;
        if (elementType.IsPointer)
            return default;

        int typeIndex = GetTypeIndex(elementType!, refs);

        var valueSpan = _serializedObjects.AddMultiple(array.Length + 1 + array.Rank);
        SerialHandle rank = default;
        rank.SerialType = SerialHandle.Type.ArrayCount;
        rank.Handle = array.Rank;
        rank.DebugTypeName = "array rank";
        rank.IndexedType = typeIndex;

        valueSpan[0] = rank;
        for(int dim = 0; dim < array.Rank; dim++)
        {
            SerialHandle dimensionLength = default;
            dimensionLength.SerialType = SerialHandle.Type.ArrayCount;
            dimensionLength.Handle = array.GetLength(dim);
            dimensionLength.DebugTypeName = "dimension length";
            dimensionLength.IndexedType = typeIndex;
            valueSpan[1+dim] = dimensionLength;
        }

        if(array.Rank == 1)
        {
            for (int i = 0; i < array.Length; i++)
            {
                var val = array.GetValue(i);
                if (val != null)
                {
                    var handle = SerializeRecursive(val, res.Handle + 2 + i, refs);
                    valueSpan[2 + i] = handle;
                }
            }
            return res;
        }
        return default;//throw new ArgumentException("I am not serializing multi-dimensional arrays right now.");
    }
    
    SerialHandle WritePrimitive(object obj, in SerializationRefs refs)
    {
        SerialHandle res = default;
        res.SerialType = SerialHandle.Type.Primitive;
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
        
        return res; // im not serializing decimal or pointers ig

        void WriteInt<T>(T num, in SerializationRefs refs, int overrideSizeof = -1) where T : unmanaged, IBinaryInteger<T>
        {
            int ssize = overrideSizeof < 0 ? Unsafe.SizeOf<T>() : overrideSizeof;
            res.IndexedType = GetTypeIndex(typeof(T), refs);
            res.DebugTypeName = typeof(T).Name;

            var span = _primitiveData.AddMultiple(ssize);
            num.TryWriteLittleEndian(span, out _);
        }
        unsafe void WriteFloat<T>(T num, in SerializationRefs refs) where T : unmanaged, IBinaryFloatingPointIeee754<T>
        {
            int ssize = Unsafe.SizeOf<T>();
            res.IndexedType = GetTypeIndex(typeof(T), refs);
            res.DebugTypeName = typeof(T).Name;

            var span = _primitiveData.AddMultiple(ssize);

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

    ref struct SerializationRefs
    {
        public Dictionary<object, int> ReferenceIDs;
        public Dictionary<Type, (int, ReadOnlyCollection<FieldInfo>, ReadOnlyCollection<MethodInfo>)> TypeIndices;
    }
}