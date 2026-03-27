using System;
using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.Graphics;
using SlopperEngine.Graphics.DefaultResources;
using SlopperEngine.Rendering;
using SlopperEngine.SceneObjects;
using SlopperEngine.Windowing;

namespace TestProgram.PerformanceTests;

public class RenderDemo : SceneObject, IDemo
{
    Window _displayWindow;
    double _fpscapBeforeDemo;

    public static Scene CreateDemoScene()
    {
        Scene scene = Scene.CreateDefault();
        scene.Children.Add(new RenderDemo(scene));
        return scene;
    }

    private RenderDemo(Scene scene)
    {
        _displayWindow = Window.Create(new((1200,800), Title:"Performance tests"));
        _displayWindow.CenterWindow();
        _displayWindow.Scene = scene;
        _displayWindow.WindowTexture = scene.SceneRenderer?.GetOutputTexture();
        _displayWindow.Closing += a => scene.Destroy();
        scene.SceneRenderer?.Resize((1200,800));

        // set fps cap infinite to test it properly
        _fpscapBeforeDemo = MainContext.Instance.UpdateFrequency;
        MainContext.Instance.UpdateFrequency = 0;
        scene.OnDestroy += () => MainContext.Instance.UpdateFrequency = _fpscapBeforeDemo;

        Camera cam = new();
        scene.Children.Add(cam);
        cam.Children.Add(new NoclipController());
        cam.Projection = Matrix4.CreatePerspectiveFieldOfView(1, 1.5f, 0.2f, 200f);

        Random rand = new(6767);
        Material mat = Material.Create(SlopperShader.Create(Asset.GetEngineAsset("shaders/phongShader.sesl")));
        for(int i = 0; i<100; i++)
        {
            MeshRenderer rend = new()
            {
                Mesh = ((uint)rand.Next() % 3) switch
                {
                   1u => DefaultMeshes.Plane,
                   2u => DefaultMeshes.Sphere,
                   _ => DefaultMeshes.Cube, 
                },
                LocalPosition = new(rand.NextSingle()*20 - 10, rand.NextSingle()*20 - 10, rand.NextSingle()*20 - 10),
                LocalRotation = Quaternion.FromEulerAngles(rand.NextSingle(), rand.NextSingle(), rand.NextSingle()),
                Material = mat
            };
            scene.Children.Add(rend);
        }
        scene.Children.Add(new PointLight()
        {
           LocalPosition = new(12,12,12),
           Color = new(1,3,10),
           Radius = 40,
           Sharpness = 1.5f 
        });
        scene.Children.Add(new PointLight()
        {
           LocalPosition = new(-12,-12,-12),
           Color = new(10,3,1),
           Radius = 40,
           Sharpness = 1.5f 
        });
    }

    [OnFrameUpdate]
    void FrameUpdate(FrameUpdateArgs args)
    {
        // record frametime
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

    static string? IDemo.GetDescription() => "Demo for testing new render features for the engine. \nPress 'Esc' to close.";
}