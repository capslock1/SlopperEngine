using System;

namespace SlopperEngine.Core.Serialization;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class DontSerializeAttribute : Attribute
{

}