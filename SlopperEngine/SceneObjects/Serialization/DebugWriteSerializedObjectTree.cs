namespace SlopperEngine.SceneObjects.Serialization;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Reflection;

public partial class SerializedObjectTree
{
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
}