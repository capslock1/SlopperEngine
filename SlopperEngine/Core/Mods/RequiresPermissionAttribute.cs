using System;

namespace SlopperEngine.Core.Mods;

/// <summary>
/// Used to mark when a member needs a special permission to be used by mods.
/// </summary>
[AttributeUsage(AttributeTargets.Class | 
                AttributeTargets.Struct | 
                AttributeTargets.Enum | 
                AttributeTargets.Constructor | 
                AttributeTargets.Method | 
                AttributeTargets.Field | 
                AttributeTargets.Property |
                AttributeTargets.Interface, 
                AllowMultiple = false, 
                Inherited = true)]
public class RequiresPermissionAttribute : Attribute
{
    /// <summary>
    /// The permission that is required to accesss the member.
    /// </summary>
    public readonly ModPermissionFlags RequiredPermissions;

    /// <param name="requiredPermissions">The permission that is required to accesss the member.</param>
    public RequiresPermissionAttribute(ModPermissionFlags requiredPermissions)
    {
        RequiredPermissions = requiredPermissions;
    }
}