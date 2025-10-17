using System;

namespace SlopperEngine.EditorIntegration;

/// <summary>
/// Indicates whether or not the field or property may be edited. If set to false, it can still be read.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class EditableInInspectorAttribute : Attribute
{
    /// <summary>
    /// Whether or not the field or property may be edited.
    /// </summary>
    public readonly bool Editable;

    /// <param name="editable">Whether or not the field or property may be edited.</param>
    public EditableInInspectorAttribute(bool editable)
    {
        Editable = editable;
    }
}