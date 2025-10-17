using System.Collections.ObjectModel;
using System.Reflection;
using SlopperEngine.SceneObjects.Serialization;
using SlopperEngine.Core.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlopperEngine.Core.Serialization;

/// <summary>
/// Caches reflection info used by the serialization system.
/// </summary>
public static class ReflectionCache
{
    static FridgeDictionary<Type, ReadOnlyCollection<FieldInfo>> _fieldInfos = new();
    static FridgeDictionary<Type, ReadOnlyCollection<MethodInfo>> _onSerializes = new();
    static MethodInfo? _deserialFromKeyMethod;
    static object?[] _singleObject = new object[1];
    static Type?[] _twoTypes = new Type[2];

    /// <summary>
    /// Tries to call "object Deserialize(TKey)" on a type that implements ISerializableFromKey.
    /// </summary>
    /// <param name="keyType">The type of the key.</param>
    /// <param name="objectType">The type of the object (that implements ISerializableFromKey).</param>
    /// <param name="key">The key to deserialize from.</param>
    /// <returns>A new instance of an object created from the key, or null if any error occurs.</returns>
    public static object? DeserializeObjectFromKey(Type keyType, Type objectType, object? key)
    {
        _deserialFromKeyMethod ??= typeof(ReflectionCache).GetMethod("DeserializeFromKey", BindingFlags.Static | BindingFlags.NonPublic);
        try
        {
            _twoTypes[0] = keyType;
            _twoTypes[1] = objectType;
            var madeMethod = _deserialFromKeyMethod!.MakeGenericMethod(_twoTypes!);
            _singleObject[0] = key;
            return madeMethod.Invoke(null, _singleObject);
        }
        catch (ArgumentException)
        {
            _singleObject[0] = null; // cut the gc a little slack
            _twoTypes[0] = null;
            _twoTypes[1] = null;
            return null;
        }
    }
    static object? DeserializeFromKey<T, TSerial>(object key) where TSerial : ISerializableFromKey<T> => TSerial.Deserialize((T)key);

    /// <summary>
    /// Gets the allowed serializable fields for a given type.
    /// </summary>
    public static ReadOnlyCollection<FieldInfo> GetSerializableFields(Type type)
    {
        if (_fieldInfos.TryGetValue(type, out var res))
            return res;

        var currTyp = type;
        List<FieldInfo> allowedFields = new();
        while (currTyp != null)
        {
            var fields = currTyp.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (var f in fields)
                if (!f.GetCustomAttributes(typeof(DontSerializeAttribute)).Any())
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
        if (_onSerializes.TryGetValue(type, out var res))
            return res;

        var list = new List<MethodInfo>();
        RecursiveAddMethods(type);

        void RecursiveAddMethods(Type t)
        {
            if (t.BaseType != null)
                RecursiveAddMethods(t.BaseType);

            foreach (var meth in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (meth.IsConstructor)
                    continue;
                if (meth.IsGenericMethod)
                    continue;

                if (!meth.GetCustomAttributes(typeof(OnSerializeAttribute)).Any())
                    continue;
                var parameters = meth.GetParameters();
                if (parameters.Length != 1)
                {
                    System.Console.WriteLine($"OnSerialize expects only a single parameter at {t.Name}.{meth.Name}().");
                    continue;
                }
                if (meth.IsVirtual)
                {
                    System.Console.WriteLine($"OnSerialize expects only non-virtual methods at {t.Name}.{meth.Name}().");
                    continue;
                }
                if (parameters[0].ParameterType != typeof(OnSerializeArgs))
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