using SlopperEngine.SceneObjects;

namespace TestProgram.PerformanceTests;

public class PerformanceDemos : IDemo
{
    public static Scene CreateDemoScene()
    {
        return Scene.CreateEmpty(); // this will crash the program for now. WIP obvs
    }

    static string? IDemo.GetDescription() => "Test test test! haha";
}