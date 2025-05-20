namespace SlopperEngine.Core.Serialization;

public record struct SerialHandle
{
    public int Handle;
    public int IndexedType;
    public string? DebugTypeName;
    public Type SerialType;
    public bool SaveFields;

    public enum Type : byte
    {
        Reference, 
        ReferenceToPrevious,
        Primitive, 
        Array,
        ArrayCount,
        CustomSerializedObjects,
        CustomSerializedObjectsCount,
        OutsideReference,
        SerializedFromKey,
        KeyType,
    }
}