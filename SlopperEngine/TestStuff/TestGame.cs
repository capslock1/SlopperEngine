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
using SlopperEngine.Graphics.DefaultResources;
using SlopperEngine.Physics;
using SlopperEngine.SceneObjects.Rendering;
using SlopperEngine.Physics.Colliders;
using SlopperEngine.UI;
using SlopperEngine.UI.Base;
using SlopperEngine.UI.Text;
using SlopperEngine.Windowing;
using SlopperEngine.Core.Serialization;
using SlopperEngine.SceneObjects.Serialization;
using SlopperEngine.UI.Navigation;

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
    [DontSerialize] Material? _background;
    int _backgroundTexIndex;
    Windowing.Window _testWindow;
    
    SlopperShader? _cubeShader = null;
    Camera _camera = new ManeuverableCamera();
    Rigidbody _rb;
    TextBox? _fpsCounter;
    PointLight[] _lamps;
    UIElement _myScrollableRectangle;

    public TestGame(int width, int height, string title)
    {
        Console.WriteLine("Using " + GL.GetString(StringName.Renderer));
        Console.WriteLine("Maximum frame buffer color attachments: " + GL.GetInteger(GetPName.MaxColorAttachments));
        Console.WriteLine("Maximum uniform buffers: " + GL.GetInteger(GetPName.MaxUniformBufferBindings));
        Console.WriteLine("Maximum ssbos: " + GL.GetInteger(GetPName.MaxShaderStorageBufferBindings));
        Console.WriteLine("Maximum array tex layers: " + GL.GetInteger(GetPName.MaxArrayTextureLayers));
        Console.WriteLine("Maximum texture units: " + GL.GetInteger(GetPName.MaxTextureImageUnits));
        Console.WriteLine("Maximum compute image units: " + GL.GetInteger(GetPName.MaxComputeImageUniforms));
        Console.WriteLine($"Using OpenGL version: {GLInfo.VersionString}, as int: {GLInfo.Version}");

        MainContext.ThrowIfSevereGLError = true;
        MainContext.Instance.UpdateFrequency = 10000;

        _main = Scene.CreateDefault();
        _main.RenderHandler!.ClearColor = new(0, 0, 0, 1);

        Matrix4 cameraProjection = Matrix4.CreatePerspectiveFieldOfView(.9f, width / (float)height, .1f, 1000f);
        _camera.Projection = cameraProjection;
        _camera.LocalPosition = (0, 1, 3);
        _main.Children.Add(_camera);

        _cubeShader = SlopperShader.Create("shaders/phongShader.sesl");

        MeshRenderer sphere = new()
        {
            Mesh = DefaultMeshes.Sphere,
            Material = Material.Create(_cubeShader),
            LocalPosition = new Vector3(0, -1.4f, 0),
        };
        var sphereCol = new Rigidbody();
        sphereCol.Colliders.Add(new SphereCollider(1, .5f));
        sphereCol.IsKinematic = true;
        sphereCol.Children.Add(sphere);

        MeshRenderer missing = new()
        {
            LocalPosition = new Vector3(0, 0, -3),
        };

        var rand = new System.Random();
        _main.Children.Add(new Plimbo());
        _main.Children.Add(sphereCol);
        _main.Children.Add(missing);

        //_main.Children.Add(new RotateCubeAdder(10, 1f));
        _main.Children.Add(new ComputeShaderTest());
        //_main.Children.Add(new AddRemoveSpeedTester());

        _rb = new Rigidbody();
        _rb.Position = (2, 0, 0);
        _rb.Colliders.Add(new SphereCollider(10, 1));
        _rb.Children.Add(new MeshRenderer() { Mesh = DefaultMeshes.Sphere });
        _main.Children.Add(_rb);
		for(int x = 0; x < 100; x++){
			for(int y = 0; y < 100; y++){
				var rb3 = new Rigidbody();
				rb3.Position = (x, y, 0.1f);
				rb3.Colliders.Add(new SphereCollider(10, 1));
				rb3.Children.Add(new MeshRenderer() { Mesh = DefaultMeshes.Sphere });
				_main.Children.Add(rb3);
			}
		}
        var rb2 = new Rigidbody();
        rb2.Position = (2, 3, 0.1f);
        var ball = new SphereCollider(10, 1);
        var cube = new BoxCollider(10, new(1)) { Position = (1, 0, 0) };
        rb2.Colliders.Add(ball);
        rb2.Colliders.Add(cube);
        ball.Children.Add(new MeshRenderer() { Mesh = DefaultMeshes.Sphere });
        cube.Children.Add(new MeshRenderer() { Mesh = DefaultMeshes.Cube, LocalScale = new(.5f) });
        _main.Children.Add(rb2);

        _rb.AngularVelocity = (0, 6f, 0);
        rb2.Velocity = (0, 4, 0);

        var floor = new Rigidbody() { IsKinematic = true, Position = (0, -2, 0) };
        var floorcol = new BoxCollider(1, (10, 1, 10));
        floorcol.Children.Add(new MeshRenderer() { Mesh = DefaultMeshes.Cube, LocalScale = (5, .5f, 5), Material = sphere.Material });
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
        _UIScene.Children.Add(new MaterialRectangle(_background));
        var rend = new UIRenderer();
        _UIScene.Renderers.Add(rend);
        rend.Resize((width, height));

        _fpsCounter = new();
        _fpsCounter.LocalShape = new(0, 1, 0, 1);
        _fpsCounter.Horizontal = Alignment.Max;
        _fpsCounter.Vertical = Alignment.Min;
        _fpsCounter.Text = "FPS: ";
        _fpsCounter.Scale = 2;
        _fpsCounter.BackgroundColor = new(0, 0, 0, .4f);
        _UIScene.Children.Add(_fpsCounter);

        var fonttest = new TextBox();
        fonttest.LocalShape = new(1, 1, 1, 1);
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
        fonttest.BackgroundColor = new(0, 0, 0, .4f);
        _UIScene.Children.Add(fonttest);

        var myText = new TextField(30);
        myText.TextRenderer.Scale = 1;
        myText.LocalShape = new(.5f, .5f, .5f, .5f);
        myText.TextRenderer.BackgroundColor = new(0, 0, 0, .3f);
        myText.UpdateTextRenderer();
        _UIScene.Children.Add(myText);

        var scrollable = new ScrollableArea(new(0.1f, 0.1f, 0.4f, 0.4f));
        _myScrollableRectangle = new MaterialRectangle(new(0, 0, 1.5f, 1.6f), Material.MissingMaterial);
        scrollable.UIChildren.Add(_myScrollableRectangle);
        _UIScene.Children.Add(scrollable);

        _lamps = new PointLight[10];
        var lampShader = SlopperShader.Create("shaders/UnlitColor.sesl");
        for (int i = 0; i < _lamps.Length; i++)
        {
            var color = new Vector3(rand.NextSingle() * 5, rand.NextSingle() * 5, rand.NextSingle() * 5);
            var lamp = new PointLight() { LocalPosition = new(1, 3, 1), Color = color, Radius = 20, Sharpness = 2.5f };
            var lampMat = Material.Create(lampShader);
            lampMat.Uniforms[lampMat.GetUniformIndexFromName("Color")].Value = color * 9;
            lamp.Children.Add(new MeshRenderer() { Mesh = DefaultMeshes.Sphere, Material = lampMat, LocalScale = new(.2f) });
            _main.Children.Add(lamp);
            _lamps[i] = lamp;
        }

        _main.FrameUpdate(new(.0001f));

        StbImage.stbi_set_flip_vertically_on_load(0);
        _testWindow = Windowing.Window.Create(new(
            (width, height),
            default,
            title,
            WindowState.Normal,
            true,
            false,
            new(new OpenTK.Windowing.Common.Input.Image(
                32,
                32,
                ImageResult.FromStream(
                    File.OpenRead(Assets.GetPath("defaultTextures/logo.png")),
                    ColorComponents.RedGreenBlueAlpha).Data)))
            );

        _testWindow.Scene = _main;
        _testWindow.WindowTexture = _UIScene.Renderers.FirstOfType<UIRenderer>()?.GetOutputTexture();
        _background.Uniforms[_backgroundTexIndex].Value = _main.RenderHandler!.GetOutputTexture();
        _testWindow.FramebufferResize += OnFramebufferResize;
    }

    [OnSerialize] void OnSerialize(SerializedObjectTree.CustomSerializer tree)
    {
        if(tree.IsReader) return;

        _background = Material.Create(SlopperShader.Create("shaders/UI/PostProcessing.sesl"));
        _backgroundTexIndex = _background.GetUniformIndexFromName("sourceTexture");
        _UIScene = Scene.CreateEmpty();
        _UIScene.Components.Add(new UpdateHandler());
        _UIScene.Children.Add(new MaterialRectangle(_background));
        var rend = new UIRenderer();
        _UIScene.Renderers.Add(rend);
        rend.Resize(_testWindow.ClientSize);

        _fpsCounter = new();
        _fpsCounter.LocalShape = new(0,1,0,1);
        _fpsCounter.Horizontal = Alignment.Max;
        _fpsCounter.Vertical = Alignment.Min;
        _fpsCounter.Text = "FPS: ";
        _fpsCounter.Scale = 2;
        _fpsCounter.BackgroundColor = new(0,0,0,.4f);
        _UIScene.Children.Add(_fpsCounter);

        tree.CallAfterSerialize(() =>
        {
            _testWindow.Scene = _main!;
            _testWindow.WindowTexture = rend.GetOutputTexture();
            _background.Uniforms[_backgroundTexIndex].Value = _main!.RenderHandler!.GetOutputTexture();
            _testWindow.FramebufferResize += OnFramebufferResize;
        });
    }

    [OnFrameUpdate]
    void OnUpdateFrame(FrameUpdateArgs args)
    {
        _secsSinceStart += args.DeltaTime;
        _framesThisSecond++;
        if (_secsSinceStart > _intSecsSinceStart + 1)
        {
            _fpsCounter!.Text = $"FPS: {_framesThisSecond}";
            _intSecsSinceStart++;
            _framesThisSecond = 0;
        }

        _myScrollableRectangle.LocalShape = new(0, 0, 1 + MathF.Sin((float)_secsSinceStart * 0.224f), 1 + MathF.Sin((float)_secsSinceStart * .197f));

        _rb.AddImpulse(45 * args.DeltaTime * Vector3.UnitY * (MathF.Sin((float)_secsSinceStart)+1));
        Random rand = new(203958);
        foreach(var lamp in _lamps)
            lamp!.LocalPosition = new(
                13*(float)Math.Sin(.1*_secsSinceStart + 1000*rand.NextDouble()), 
                1 + 4*rand.NextSingle(), 
                13*(float)Math.Cos(.1*_secsSinceStart * Math.Sqrt(2) + 1000*rand.NextDouble()));
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

        foreach(var rend in _main!.Renderers.AllOfType<RenderHandler>())
            rend.Resize(e.Size);
        foreach(var rend in _UIScene!.Renderers.AllOfType<RenderHandler>())
            rend.Resize(e.Size);

        _background!.Uniforms[_backgroundTexIndex].Value = _main.RenderHandler?.GetOutputTexture();
        _testWindow.WindowTexture = _UIScene.Renderers.FirstOfType<UIRenderer>()?.GetOutputTexture();
    }
}
