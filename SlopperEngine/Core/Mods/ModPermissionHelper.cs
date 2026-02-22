using System;
using System.Collections.Generic;
using System.Reflection;
using DouglasDwyer.CasCore;

namespace SlopperEngine.Core.Mods;

/// <summary>
/// Helper functions to aid with ModPermissionFlags.
/// </summary>
[RequiresPermission(ModPermissionFlags.ManageMods)]
public static class ModPermissionHelper
{
    static readonly Dictionary<ModPermissionFlags, CasPolicy> _policyCache = new();

    /// <summary>
    /// Tests whether or not a mod has higher permissions than another mod.
    /// </summary>
    /// <param name="modPermissions">The mod to test permissions of.</param>
    /// <param name="permissionToTest">The permissions to test for.</param>
    /// <returns>Whether or not a mod has permissions to do something.</returns>
    public static bool HasPermissions(ModPermissionFlags modPermissions, ModPermissionFlags permissionToTest)
    {
        if((modPermissions & ModPermissionFlags.Unrestricted) == ModPermissionFlags.Unrestricted) return true;
        return modPermissions.HasFlag(permissionToTest);
    }

    /// <summary>
    /// Gets the associated CasPolicy for a given ModPermissionFlags.
    /// </summary>
    public static CasPolicy GetPolicy(ModPermissionFlags permissions)
    {
        lock(_policyCache)
        {
            if(_policyCache.TryGetValue(permissions, out var result))
                return result;
        }

        var policy = new CasPolicyBuilder().WithDefaultSandbox();
        
        if(permissions.HasFlag(ModPermissionFlags.Default))
        {
            var slopperEngine = Assembly.GetExecutingAssembly();
            policy.Allow(new AssemblyBinding(slopperEngine, Accessibility.Protected));
            DenyMissingPermissionsOfMembers(policy, slopperEngine, permissions);

            // i think all of these are fairly safe. assimp and cas are really not necessary to allow because they get abstracted away
            policy.Allow(new AssemblyBinding(typeof(OpenTK.IBindingsContext).Assembly, Accessibility.Protected));
            policy.Allow(new AssemblyBinding(typeof(BepuPhysics.ActiveConstraintBodyHandleCollector).Assembly, Accessibility.Protected));
        }

        if(permissions.HasFlag(ModPermissionFlags.AccessNetwork))
        {
            policy.Allow(new AssemblyBinding(Assembly.Load("System.Net"), Accessibility.Public));
        }

        if(permissions.HasFlag(ModPermissionFlags.AccessOtherMods))
        {
            foreach(var mod in SlopModInfo.GetAllMods())
            {
                if(mod.IsSlopperEngine) continue;
                foreach(var assembly in mod.AssembliesInMod)
                {
                    policy.Allow(new AssemblyBinding(assembly, Accessibility.Protected)); 
                    DenyMissingPermissionsOfMembers(policy, assembly, permissions);
                }
            }
        }
        
        var res = policy.Build();
        if(!permissions.HasFlag(ModPermissionFlags.AccessOtherMods)) // cannot be cached, as new mods are constantly added   
            lock(_policyCache)
                _policyCache[permissions] = res;
        return res;
    }

    // denies all applicable members decorated with [RequiresPermission()]
    static void DenyMissingPermissionsOfMembers(CasPolicyBuilder policy, Assembly assembly, ModPermissionFlags permissions)
    {
        foreach(var t in assembly.GetTypes())
        {
            var typePermission = t.GetCustomAttribute<RequiresPermissionAttribute>();
            if(typePermission != null && !permissions.HasFlag(typePermission.RequiredPermissions))
                policy.Deny(new TypeBinding(t, Accessibility.Private));
            
            foreach(var mem in t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                if(mem is Type) 
                    continue; // nested types are handled by Assembly.GetTypes()

                var memPermission = mem.GetCustomAttribute<RequiresPermissionAttribute>();
                if(memPermission != null && !permissions.HasFlag(memPermission.RequiredPermissions))
                    policy.Deny(mem);
            }
        }
    }
}