namespace SlopperEngine.Core.Mods;

/// <summary>
/// Helper functions to aid with ModPermissionFlags.
/// </summary>
public static class ModPermissionHelper
{
    /// <summary>
    /// Tests whether or not a mod has higher permissions than another mod.
    /// </summary>
    /// <param name="modPermissions">The mod to test permissions of.</param>
    /// <param name="permissionToTest">The permissions to test for.</param>
    /// <returns>Whether or not a mod has permissions to do something.</returns>
    public static bool HasPermissions(ModPermissionFlags modPermissions, ModPermissionFlags permissionToTest)
    {
        if((modPermissions & ModPermissionFlags.Unrestricted) == ModPermissionFlags.Unrestricted) return true;
        var combinedPermissions = modPermissions | permissionToTest;
        return combinedPermissions == modPermissions; // if permissionToTest has permissions that the mod does not, this is false
    }
}