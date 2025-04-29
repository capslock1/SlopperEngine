using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using SlopperEngine.Graphics;
using SlopperEngine.SceneObjects;
using StbImageSharp;
using SlopperEngine.Core.SceneComponents;
using SlopperEngine.Core;
using SlopperEngine.Graphics.Renderers;
using SlopperEngine.Physics;
using SlopperEngine.SceneObjects.Rendering;
using SlopperEngine.Physics.Colliders;
using SlopperEngine.UI;
using SlopperEngine.Windowing;

namespace SlopperEngine.TestStuff;

/// <summary>
/// The testing zone for SlopperEngine scenes.
/// </summary>
public class TestGame : SceneObject
{
    double _secsSinceStart = 0;
    int _intSecsSinceStart = 0;
    int _framesThisSecond = 0;

    Scene? _main;
    Scene? _UIScene;
    Material? _background;
    int _backgroundTexIndex;
    Windowing.Window _testWindow;
    
    SlopperShader? _cubeShader = null;
    Camera _camera = new ManeuverableCamera();
    Rigidbody _rb;
    TextBox _fpsCounter;

    public TestGame(int width, int height, string title)
    {
        Console.WriteLine("Using "+GL.GetString(StringName.Renderer));
        Console.WriteLine("Maximum frame buffer color attachments: "+GL.GetInteger(GetPName.MaxColorAttachments));
        Console.WriteLine("Maximum uniform buffers: "+GL.GetInteger(GetPName.MaxUniformBufferBindings));
        Console.WriteLine("Maximum array tex layers: "+GL.GetInteger(GetPName.MaxArrayTextureLayers));
        Console.WriteLine("Maximum texture units: "+GL.GetInteger(GetPName.MaxTextureImageUnits));
        Console.WriteLine("Maximum compute image units: "+GL.GetInteger(GetPName.MaxComputeImageUniforms));
        Console.WriteLine($"Using OpenGL version: {GLInfo.VersionString}, as int: {GLInfo.Version}");
        
        MainContext.ThrowIfSevereGLError = true;

        _main = Scene.CreateDefault();
        _main.RenderHandler!.ClearColor = new(0,0,0,1);

        Matrix4 cameraProjection = Matrix4.CreatePerspectiveFieldOfView(.9f, width/(float)height, .1f, 1000f);
        _camera.Projection = cameraProjection;
        _camera.LocalPosition = (0,1,3);
        _main.Children.Add(_camera);
        
        _cubeShader = SlopperShader.Create("shaders/phongShader.sesl");
        
        MeshRenderer sphere = new()
        {
            Mesh = DefaultMeshes.Sphere,
            Material = Material.Create(_cubeShader),
            LocalPosition = new Vector3(0,-1.4f,0),
        };
        var sphereCol = new Rigidbody();
        sphereCol.Colliders.Add(new SphereCollider(1,.5f));
        sphereCol.IsKinematic = true;
        sphereCol.Children.Add(sphere);

        MeshRenderer missing = new()
        {
            LocalPosition = new Vector3(0,0,-3),
        };

        //addOneMillionBillionCubes();

        var rand = new System.Random();
        _main.Children.Add(new Plimbo());
        _main.Children.Add(sphereCol);
        _main.Children.Add(missing);

        //_main.Children.Add(new StandalonePhysTest());
        //_main.Children.Add(new RotateCubeAdder(10, 1f));
        _main.Children.Add(new ComputeShaderTest());
        //_main.Children.Add(new AddRemoveSpeedTester());
        //camera.AddChild(new OpenALtest());
        //main.AddChild(new DiscordTest());

        _rb = new Rigidbody();
        _rb.Position = (2,0,0);
        _rb.Colliders.Add(new SphereCollider(10,1));
        _rb.Children.Add(new MeshRenderer(){Mesh = DefaultMeshes.Sphere});
        _main.Children.Add(_rb);

        var rb2 = new Rigidbody();
        rb2.Position = (2,3,0.1f);
        var ball = new SphereCollider(10,1);
        var cube = new BoxCollider(10,new(1)){Position = (1,0,0)};
        rb2.Colliders.Add(ball);
        rb2.Colliders.Add(cube);
        ball.Children.Add(new MeshRenderer(){Mesh = DefaultMeshes.Sphere});
        cube.Children.Add(new MeshRenderer(){Mesh = DefaultMeshes.Cube, LocalScale = new(.5f)});
        _main.Children.Add(rb2);
        
        _rb.AngularVelocity = (0,6f,0);
        rb2.Velocity = (0,4,0);

        var floor = new Rigidbody(){IsKinematic = true, Position = (0,-2,0)};
        var floorcol = new BoxCollider(1, (10,1,10));
        floorcol.Children.Add(new MeshRenderer(){Mesh = DefaultMeshes.Cube, LocalScale = (5,.5f,5)});
        floor.Colliders.Add(floorcol);
        _main.Children.Add(floor);

        _main.Children.Add(this);

        var holdup = new MeshRenderer();
        _main.Children.Add(holdup);
        holdup.Remove();

        _background = Material.Create(SlopperShader.Create("shaders/UI/PostProcessing.sesl"));
        _backgroundTexIndex = _background.GetUniformIndexFromName("sourceTexture");
        _UIScene = Scene.CreateEmpty();
        _UIScene.Components.Add(new UpdateHandler());
        _UIScene.Children.Add(new MaterialQuad(_background));
        var rend = new UIRenderer();
        _UIScene.Components.Add(rend);
        rend.Resize((width, height));

        _fpsCounter = new();
        _fpsCounter.LocalShape = new(0,1,0,1);
        _fpsCounter.Horizontal = Alignment.Max;
        _fpsCounter.Vertical = Alignment.Min;
        _fpsCounter.Text = "FPS: ";
        _fpsCounter.Scale = 2;
        _fpsCounter.BackgroundColor = new(0,0,0,.4f);
        _UIScene.Children.Add(_fpsCounter);

        var fonttest = new TextBox();
        fonttest.LocalShape = new(1,1,1,1);
        fonttest.Horizontal = Alignment.Min;
        fonttest.Text = @"    
    /// <summary>
    /// How to vertically align the box. Min (downward) on default.
    /// </summary>
    public Alignment Vertical = Alignment.Min;

    public TextBox() : base(){}
    public TextBox(string text) : base()
    {
        Text = text;
    }

    string _currentText = string.Empty;
    RasterFont _currentFont = RasterFont.EightXSixteen;
    Texture2D? _texture; ä
    ";
        fonttest.Font = RasterFont.EightXSixteen;
        fonttest.Scale = 1;
        fonttest.BackgroundColor = new(0,0,0,.4f);
        _UIScene.Children.Add(fonttest);

        _main.FrameUpdate(new(.0001f));

        StbImage.stbi_set_flip_vertically_on_load(0);
        _testWindow = Windowing.Window.Create(
            (width, height), 
            null, 
            title, 
            WindowState.Normal, 
            true, 
            false, 
            new(new OpenTK.Windowing.Common.Input.Image(
                32, 
                32, 
                ImageResult.FromStream(
                    File.OpenRead(Assets.GetPath("defaultTextures/logo.png")),
                    ColorComponents.RedGreenBlueAlpha).Data))
            );

        _testWindow.Scene = _main;
        _testWindow.WindowTexture = _UIScene.Components.FirstOfType<UIRenderer>()?.GetOutputTexture();
        _background.Uniforms[_backgroundTexIndex].Value = _main.RenderHandler!.GetOutputTexture();
        _testWindow.FramebufferResize += OnFramebufferResize;

    }

    [OnFrameUpdate]
    void OnUpdateFrame(FrameUpdateArgs args)
    {
        _secsSinceStart += args.DeltaTime;
        _framesThisSecond++;
        if(_secsSinceStart > _intSecsSinceStart+1)
        {
            _fpsCounter.Text = $"FPS: {_framesThisSecond}";
            _intSecsSinceStart++;
            _framesThisSecond = 0;
        }

        _rb.AddImpulse(45 * args.DeltaTime * Vector3.UnitY * (MathF.Sin((float)_secsSinceStart)+1));
    }

    [OnInputUpdate]
    void OnInput(InputUpdateArgs args)
    {
        _UIScene?.InputUpdate(args);
        if (args.KeyboardState.IsKeyDown(Keys.Escape))
            _testWindow.Close();
    }

    void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        _camera.Projection = Matrix4.CreatePerspectiveFieldOfView(.6f, e.Width/(float)e.Height, .1f, 1000f);

        foreach(var rend in _main!.Components.AllOfType<RenderHandler>())
            rend.Resize(e.Size);
        foreach(var rend in _UIScene!.Components.AllOfType<RenderHandler>())
            rend.Resize(e.Size);

        _background!.Uniforms[_backgroundTexIndex].Value = _main.RenderHandler?.GetOutputTexture();
        _testWindow.WindowTexture = _UIScene.Components.FirstOfType<UIRenderer>()?.GetOutputTexture();
    }
}
