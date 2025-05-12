using System.Collections.ObjectModel;
using System.Reflection;
using SlopperEngine.SceneObjects.Serialization;
using SlopperEngine.Core.Collections;

namespace SlopperEngine.Core.Serialization;

public static class ReflectionCache
{
    static FridgeDictionary<Type, ReadOnlyCollection<FieldInfo>> _fieldInfos = new();
    static FridgeDictionary<Type, ReadOnlyCollection<MethodInfo>> _onSerializes = new();

    

    /// <summary>
    /// Gets the allowed serializable fields for a given type.
    /// </summary>
    public static ReadOnlyCollection<FieldInfo> GetSerializableFields(Type type)
    {
        if(_fieldInfos.TryGetValue(type, out var res))
            return res;

        var currTyp = type; 
        List<FieldInfo> allowedFields = new();
        while(currTyp != null)
        {
            var fields = currTyp.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach(var f in fields)
                if(!f.GetCustomAttributes(typeof(DontSerializeAttribute)).Any())
                    allowedFields.Add(f);

            currTyp = currTyp.BaseType;
        }

        res = allowedFields.AsReadOnly();
        _fieldInfos.Add(type, res);
        return res;
    }

    /// <summary>
    /// Gets the first method of the type with a correct OnSerialize attribute.
    /// </summary>
    public static ReadOnlyCollection<MethodInfo> GetOnSerializeMethods(Type type)
    {
        if(_onSerializes.TryGetValue(type, out var res))
            return res;

        var list = new List<MethodInfo>();
        RecursiveAddMethods(type);

        void RecursiveAddMethods(Type t)
        {
            if(t.BaseType != null)
                RecursiveAddMethods(t.BaseType);

            foreach(var meth in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if(meth.IsConstructor)
                    continue;
                if(meth.IsGenericMethod)
                    continue;
                    
                if(!meth.GetCustomAttributes(typeof(OnSerializeAttribute)).Any())
                    continue;
                var parameters = meth.GetParameters();
                if(parameters.Length != 1)
                {
                    System.Console.WriteLine($"OnSerialize expects only a single parameter at {t.Name}.{meth.Name}().");
                    continue;
                }
                if(parameters[0].ParameterType != typeof(SerializedObjectTree.CustomSerializer))
                {
                    System.Console.WriteLine($"OnSerialize expects the paramater at {t.Name}.{meth.Name}() to be of type 'SerializedObjectTree.CustomSerializer'.");
                    continue;
                }

                list.Add(meth);
            }
        }

        res = new(list);
        _onSerializes.Add(type, res);
        return res;
    }
}