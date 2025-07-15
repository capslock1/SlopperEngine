namespace SlopperEngine.EditorIntegration;

/// <summary>
/// Hides a public property or field from the editor.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class HideInInspectorAttribute : Attribute
{

}