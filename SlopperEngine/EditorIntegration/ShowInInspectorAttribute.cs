using System;

namespace SlopperEngine.EditorIntegration;

/// <summary>
/// Shows a private or protected property in the inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class ShowInInspectorAttribute : Attribute
{

}