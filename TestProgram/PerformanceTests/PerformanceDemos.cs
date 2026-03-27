using SlopperEngine.Core;
using SlopperEngine.SceneObjects;
using SlopperEngine.Windowing;

namespace TestProgram.PerformanceTests;

public class PerformanceDemos : SceneObject, IDemo
{
    Window _displayWindow;

    public static Scene CreateDemoScene()
    {
        Scene scene = Scene.CreateDefault();
        scene.Children.Add(new PerformanceDemos(scene));
        return scene;
    }

    private PerformanceDemos(Scene scene)
    {
        _displayWindow = Window.Create(new((600,400), Title:"Performance tests"));
        _displayWindow.CenterWindow();
        _displayWindow.Scene = scene;
        _displayWindow.WindowTexture = scene.SceneRenderer?.GetOutputTexture();
    }

    [OnInputUpdate]
    void InputUpdate(InputUpdateArgs args)
    {
        if(args.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
        {
            Scene?.Destroy();
            _displayWindow.Close();
        }
    }

    static string? IDemo.GetDescription() => "Demo for testing the speed of several parts of the engine. \nPress 'Esc' to close.";
}