using SlopperEngine.Core;
using SlopperEngine.Graphics;
using SlopperEngine.UI;
using SlopperEngine.Windowing;
using SlopperEngine.SceneObjects;
using SlopperEngine.Graphics.Renderers;
using SlopperEngine.SceneObjects.Rendering;
using OpenTK.Windowing.Common;
using SlopperEngine.Core.SceneComponents;
using StbImageSharp;
using OpenTK.Mathematics;
using SlopperEngine.Graphics.Loaders;
using SlopperEngine.TestStuff;

namespace SlopperEngine.SillyDemos;

/// <summary>
/// Creates a new scene and window for the demos on constructor call.
/// </summary>
public class Demos : ImageRectangle
{
    List<(Window window, Vector2 size, Vector2 position, float delay)> _additionalWindows = new();
    OpenTK.Windowing.Common.Input.Image _image;
    Window _mainWindow;
    Vector2i _maxWindowSize = new(500,375); // 4:3 ratio because im so retro

    // in this setup, we let the maincontext create this demos object and add it to a new scene immediately
    // this is a little cursed but its fairly easy for these hardcoded demos
    // eventually when this engine has an actual editor, this will be phased out for just letting the maincontext load a scene from disk instead
    public Demos()
    {
        // throwing on severe errors for easier debugging
        MainContext.ThrowIfSevereGLError = true;
        Texture = TextureLoader.FromFilepath(Assets.GetPath("defaultTextures/logo.png"));

        // simple scene with just the logo in there
        var mainScene = Scene.CreateEmpty();
        mainScene.Children.Add(this);
        mainScene.Renderers.Add(new UIRenderer());
        mainScene.Components.Add(new UpdateHandler());

        StbImage.stbi_set_flip_vertically_on_load(0);
        _image = new OpenTK.Windowing.Common.Input.Image(32,32, 
                ImageResult.FromStream(
                    File.OpenRead(Assets.GetPath("defaultTextures/logo.png")),
                    ColorComponents.RedGreenBlueAlpha).Data);

        _mainWindow = CreateWindow<UIRenderer>(mainScene, (256,256), true);
    }

    // creates a simple undecorated window and attaches the scene's renderer's texture.
    Window CreateWindow<TRenderer>(Scene scene, Vector2i size, bool keepalive = false) where TRenderer : RenderHandler
    {
        var window = Window.Create(new(size, StartVisible:false, Border: WindowBorder.Hidden, Icon: new(_image)));
        window.Scene = scene;
        window.WindowTexture = scene.Renderers.FirstOfType<TRenderer>()!.GetOutputTexture();
        window.CenterWindow();
        window.IsVisible = true;
        window.KeepProgramAlive = keepalive;
        return window;
    }

    [OnFrameUpdate] void Frame(FrameUpdateArgs args)
    {
        // move the windows in a circle around the central one.
        for(int w = 0; w<_additionalWindows.Count; w++)
        {
            var win = _additionalWindows[w];

            win.delay -= args.DeltaTime;
            if(win.delay > 0)
            {
                // cant ref a list so i keep having to set it back...
                _additionalWindows[w] = win;
                continue;
            }

            win.size += (_maxWindowSize - win.size) * (1-MathF.Exp(-1*args.DeltaTime));
            Vector2i realSize = (Vector2i)win.size;
            if(realSize != win.window.ClientSize)
                win.window.ClientSize = realSize;
            
            // magic values are cool because casting spells is awesome
            float rad = -w*MathF.Tau/_additionalWindows.Count + Scene?.UpdateHandler?.TimeMilliseconds*.0001f ?? 0;
            Vector2 location = new(MathF.Cos(rad), MathF.Sin(rad));
            location *= 400;
            win.position -= (win.position - location) * (1-MathF.Exp(-1*args.DeltaTime));
            Vector2i realLocation = (Vector2i) win.position - realSize/2;
            win.window.ClientLocation = realLocation + _mainWindow.ClientLocation + _mainWindow.Size/2;

            _additionalWindows[w] = win;
        }
    }
    [OnInputUpdate] void Input(InputUpdateArgs args)
    {
        // press k to spawn everything
        if(args.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.K))
        {
            // plimbo fractal scene. simply renders a shader to the window.
            {
                //create an empty scene, and give it a uirenderer
                Scene sc = Scene.CreateEmpty();
                var rend = new UIRenderer();
                sc.Renderers.Add(rend);
                rend.Resize(_maxWindowSize);

                // add the fractal. the shader/material system is awaiting a rework, so it looks "like this" for now.
                // most interesting part about this scene is the shader itself tbh
                Material fractal = Material.Create(SlopperShader.Create(Assets.GetPath("shaders/PlimbobrotSet.sesl")));
                fractal.Uniforms[fractal.GetUniformIndexFromName("mainTexture")].Value = TextureLoader.FromFilepath(Assets.GetPath("textures/croAA.png"));
                sc.Children.Add(new MaterialRectangle(fractal));
                _additionalWindows.Add((CreateWindow<UIRenderer>(sc, (1,1)), (1,1), (0,0), .2f));
            }

            // create the subway surfers scene.
            {
                // create a default scene for ease.
                Scene sc = Scene.CreateDefault();
                sc.RenderHandler!.Resize(_maxWindowSize);

                // documentation is really just in this class. try not to check it out cuz its gross
                var sub = new Subway();
                sc.Children.Add(sub);

                var plimboModel = new Plimbo();
                plimboModel.Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, 3.1415f);
                sc.Children.Add(plimboModel);
                sub.Player = plimboModel;
                
                sc.Children.Add(new Camera(){
                    LocalPosition = new(0, 6, 8),
                    LocalRotation = Quaternion.FromEulerAngles(-1,0,0),
                    Projection = Matrix4.CreatePerspectiveFieldOfView(1.2f,1,0.1f,100f),
                });
                _additionalWindows.Add((CreateWindow<DebugRenderer>(sc, (1,1)), (1,1), (0,0), .6f));
            }

            // create the dvd logo scene.
            {
                Scene sc = Scene.CreateEmpty();
                sc.Renderers.Add(new UIRenderer());
                sc.Components.Add(new UpdateHandler());
                sc.CheckCachedComponents();
                sc.RenderHandler!.ClearColor = new(0,0,0,1);
                var logo = new DVDLogo(new(.2f,.26667f));
                
                // this one is interesting, because it contains a nested scene.
                // the nested scene is properly updated by the main context, despite only being related by a lambda function and a texture.
                Scene plimboSpinScene = Scene.CreateDefault();
                plimboSpinScene.RenderHandler?.Resize(new(187,187));
                plimboSpinScene.Children.Add(new Plimbo());
                var camPivot = new SceneObject3D();
                camPivot.Children.Add(new Camera(){
                    LocalPosition = (0,0,4), 
                    Projection = Matrix4.CreatePerspectiveFieldOfView(.4f,1,0.1f,10f)});
                plimboSpinScene.Children.Add(camPivot);
                
                Random rand = new();
                int colorSel = 0;
                logo.OnBounce += () => { 
                    colorSel++;
                    colorSel %= 3;
                    // BRG color? who do i think i am
                    plimboSpinScene.RenderHandler!.ClearColor = colorSel switch{
                        0 => new(.1f,.2f,.35f, 1f), 
                        1 => new(.35f,.05f,.2f, 1f),
                        _ => new(.05f,.45f,.05f, 1f)}; 
                    camPivot.LocalRotation = Quaternion.FromEulerAngles(0, rand.NextSingle()*6.28f, 0); 
                    plimboSpinScene.FrameUpdate(new(5));
                    };

                logo.Texture = plimboSpinScene.RenderHandler?.GetOutputTexture();
                sc.Children.Add(logo);
                _additionalWindows.Add((CreateWindow<UIRenderer>(sc, (1,1)), (1,1), (0,0), .4f));
            }

            _mainWindow.Focus();
        }
        // because the windows are undecorated, they should be manually closed using escape
        if(args.KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
        {
            _mainWindow.Close();
            Scene?.Destroy();
        }
    }
}
