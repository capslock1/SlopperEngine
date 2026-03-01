using System;

namespace TestProgram;

public class Program : SlopperEngine.Core.Mods.ISlopModEvents
{
    public static void OnModLoad()
    {
        System.Console.WriteLine("Wow! This gets called! (press enter to close)");
        Console.ReadLine();
    }
}