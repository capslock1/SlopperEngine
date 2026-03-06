using System;
using SlopperEngine.Windowing;
using TestProgram.SillyDemos;

namespace TestProgram;

public class Program : SlopperEngine.Core.Mods.ISlopModEvents
{
    public static void OnModLoad()
    {
        System.Console.WriteLine("TestProgram sucessfully called!");
        try
        {
            // went on a wild goose chase trying to fix the issues this was causing earlier. 
            // multithreading should reaaaallly be fixed, but im not sure what even is the issue...
            MainContext.MultithreadedFrameUpdate = false; 
            new Demos();
        }
        catch(Exception e)
        {
            System.Console.WriteLine($"TestProgram could not run the silly demos due to an unexpected error: {e.Message}");
            System.Console.WriteLine("Press 'Enter' to quit.");
            Console.ReadLine();
        }
    }
}