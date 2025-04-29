using System.Collections.ObjectModel;
using System.Reflection;
using SlopperEngine.SceneObjects;
using SlopperEngine.Core.Collections;

namespace SlopperEngine.Core;

/// <summary>
/// Creates a central dictionary where all SceneObjects can look up and register their EngineMethods.
/// </summary>
public static class SceneObjectReflectionCache
{
    static List<Assembly> _addedAssemblies = new();
    static FridgeDictionary<Type, ReadOnlyCollection<EngineMethodAttribute>> _engineMethods = new();
    
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
    /// Adds an assembly to the EngineMethod register. If the assembly is already present, it gets ignored.
    /// </summary>
    /// <param name="ass">Assembly to add.</param>
    public static void AddAssembly(Assembly ass)
    {
        if(_addedAssemblies.Contains(ass))
        {
            Console.WriteLine($"Attempted to add assembly {ass} to the SceneObjectReflectionCache, but it was already present");
            return;
        }

        FindEngineMethods(ass);
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