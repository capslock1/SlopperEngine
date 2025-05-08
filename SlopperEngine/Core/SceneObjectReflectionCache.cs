using System.Collections.ObjectModel;
using System.Reflection;
using SlopperEngine.SceneObjects;
using SlopperEngine.Core.Collections;
using SlopperEngine.SceneObjects.Serialization;

namespace SlopperEngine.Core;

/// <summary>
/// Creates a central dictionary where all SceneObjects can look up and register their EngineMethods.
/// </summary>
public static class SceneObjectReflectionCache
{
    static List<Assembly> _addedAssemblies = new();
    static FridgeDictionary<Type, ReadOnlyCollection<EngineMethodAttribute>> _engineMethods = new();
    static FridgeDictionary<Type, ReadOnlyCollection<FieldInfo>> _fieldInfos = new();
    static FridgeDictionary<Type, ReadOnlyCollection<MethodInfo>> _onSerializes = new();
    
    /// <summary>
    /// Adds an assembly to the EngineMethod register. If the assembly is already present, it gets ignored.
    /// </summary>
    /// <param name="assembly">Assembly to add.</param>
    public static void AddAssembly(Assembly assembly)
    {
        if(_addedAssemblies.Contains(assembly))
        {
            Console.WriteLine($"Attempted to add assembly {assembly} to the SceneObjectReflectionCache, but it was already present");
            return;
        }

        FindEngineMethods(assembly);
    }

    /// <summary>
    /// Gets the engine methods associated with the type of a SceneObject.
    /// </summary>
    /// <param name="SceneObjectType"></param>
    public static ReadOnlyCollection<EngineMethodAttribute> GetEngineMethods(Type SceneObjectType)
    {
        if(_engineMethods.TryGetValue(SceneObjectType, out var res))
            return res;
        AddAssembly(SceneObjectType.Assembly);
        return _engineMethods[SceneObjectType];
    }

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
                if(!parameters[0].IsIn)
                {
                    System.Console.WriteLine($"OnSerialize expects the parameter at {t.Name}.{meth.Name} to be an 'in' parameter.");
                    continue;
                }
                if(parameters[0].ParameterType != typeof(SerializedObjectTree.CustomSerializer))
                {
                    System.Console.WriteLine($"OnSerialize expects the paramater at {t.Name}.{meth.Name} to be of type 'SerializedObjectTree.CustomSerializer'.");
                    continue;
                }

                list.Add(meth);
            }
        }

        res = new(list);
        _onSerializes.Add(type, res);
        return res;
    }

    static void FindEngineMethods(Assembly ass)
    {
        _addedAssemblies.Add(ass);
        Type[] inheriting = ass.GetTypes().Where(type => type.IsAssignableTo(typeof(SceneObject))).ToArray();
        foreach(Type scobj in inheriting)
        {
            //Console.WriteLine("- "+scobj.Name);
            _engineMethods.Add(scobj, GetMethods(scobj).AsReadOnly());
        }
    }

    static List<EngineMethodAttribute> GetMethods(Type obj)
    {
        List<EngineMethodAttribute> methodHolders = new();
        while(obj.IsAssignableTo(typeof(SceneObject)))
        {
            //Console.WriteLine("- - "+obj.Name);
            MethodInfo[] methods = obj.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach(var method in methods)
                processMethod(method);

            obj = obj.BaseType!;
        }
        return methodHolders;

        void processMethod(MethodInfo method)
        {
            //Console.WriteLine("- - - "+method.Name);
            
            foreach(Attribute attribute in Attribute.GetCustomAttributes(method, typeof(EngineMethodAttribute)))
            {
                var att = (EngineMethodAttribute)attribute;
                //Console.WriteLine("- - - - "+att.GetType());
                //Console.WriteLine("- - - - "+method.ReturnType);
                //Console.WriteLine("- - - - "+method.GetParameters().Length);

                //var handle = method.MethodHandle;
                //var functionPointer = handle.GetFunctionPointer();
                
                if(method.IsVirtual)
                {
                    Console.WriteLine($"Method {method.DeclaringType!.Name}.{method.Name}() added to {att.GetType().Name} is virtual,");
                    Console.WriteLine("meaning overriding behaviour will be ignored (and the base function will be called instead).");
                    Console.WriteLine("If this is not intended, try replacing it with a method calling the virtual method.");
                }

                try
                {
                    methodHolders.Add(att.CreateUsableFromMethodInfo(method));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Some error occurred when attempting to add {method.DeclaringType!.Name}.{method.Name}() to [{att.GetType().Name}]:");
                    Console.WriteLine(e.Message);
                }
            }
        }
    } 
}