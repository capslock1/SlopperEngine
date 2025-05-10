using System.Buffers.Binary;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
            deserializedObjects.TryGetValue(thisObject.Handle-1, out var res);
            return res;
            case SerialHandle.Type.Array:
            return ReadArray(thisObject);
            default:
            return null;
        }
    }

    public void WriteOutTree()
    {
        (Type root, ReadOnlyCollection<FieldInfo?> fields, ReadOnlyCollection<MethodInfo?> onSerializeMethods) = _indexedTypes[0];

        TextWriter w = new StringWriter();
        IndentedTextWriter writer = new(w);
        //try{
            RecursiveWriteTree(1, root, fields, writer);
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
                {
                    output.WriteLine("    (reference was null.)");
                    continue;
                }

                (Type fieldT, ReadOnlyCollection<FieldInfo?> infos, ReadOnlyCollection<MethodInfo?> onSerializeMethods) = _indexedTypes[handle.IndexedType];
                RecursiveWriteTree(handle.Handle-1, fieldT, infos, output);
            }
            else 
            switch(handle.SerialType)
            {
                case SerialHandle.Type.Primitive:
                output.Write(" (primitive): ");
                output.WriteLine(ReadPrimitive(handle) ?? "null");
                break;
                case SerialHandle.Type.Reference:
                output.WriteLine(" (reference up the tree).");
                break;
                case SerialHandle.Type.Array:
                output.WriteLine(" (yeah idk what to do with this)");
                break;
                default:
                output.WriteLine(" (unknown serial type)");
                break;
            }
        }
        output.Indent--;
    }

    Array? ReadArray(SerialHandle handle)
    {
        return null;
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
            Primitive, 
            Array,
            ArrayCount,
            CustomSerializedObjects,
            CustomSerializedObjectsCount,
        }
    }
}