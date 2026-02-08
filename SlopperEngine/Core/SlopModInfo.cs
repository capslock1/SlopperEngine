using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SlopperEngine.Core;

/// <summary>
/// Contains important information about a slopmod.
/// </summary>
public sealed class SlopModInfo
{
    static Dictionary<string, SlopModInfo> _modAtPath = new();

    SlopModInfo(Stream slopmodfile)
    {
        
    }

    public static bool TryGetInfo(string fullFilePath, [NotNullWhen(true)] SlopModInfo? result)
    {
        result = null;
        try
        {
            fullFilePath = Path.GetFullPath(fullFilePath);
        }
        catch(Exception? e)
        {
            System.Console.WriteLine($"Error loading SlopMod at '{fullFilePath}' due to: "+e.Message);
        }
        return false;
    }
}