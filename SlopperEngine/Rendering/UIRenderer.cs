using System.CodeDom.Compiler;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using SlopperEngine.Graphics.ShadingLanguage;
using SlopperEngine.Core;
using SlopperEngine.Graphics.GPUResources;
using SlopperEngine.Graphics.GPUResources.Meshes;
using SlopperEngine.Graphics.GPUResources.Textures;
using SlopperEngine.Graphics;
using SlopperEngine.UI.Base;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SlopperEngine.Rendering;

/// <summary>
/// Renders and updates UIElements.
/// </summary>
public class UIRenderer : SceneRenderer
{
    public FrameBuffer Buffer { get; private set; } = new(400, 300);

    readonly List<(Box2 shape, Material mat, Box2 scissor)> _UIElementRenderQueue = [];
    readonly ScreenspaceQuadMesh _drawMesh = new(default);
    Vector2i _screenSize = (400, 300);

    Vector2 _previousMousePosition;

    public UIRenderer()
    {
        globals.CameraProjection = Matrix4.Identity;
        globals.CameraView = Matrix4.Identity;
        globals.Model = Matrix4.Identity;
    }

    protected override void RenderInternal()
    {
        if (Scene == null) return;
        foreach (var uiRoot in Scene!.GetDataContainerEnumerable<UIRootUpdate>())
            uiRoot.UpdateShape(new(-1, -1, 1, 1), this);
        Buffer.Use();
        globals.Use();
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.Disable(EnableCap.ScissorTest);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        bool scissorActive = false;
        Box2 viewport = new(default, _screenSize);

        foreach (var uiRoot in Scene!.GetDataContainerEnumerable<UIRootUpdate>())
            uiRoot.AddRender(new(Vector2.NegativeInfinity, Vector2.PositiveInfinity), this);

        Vector2 pixelFix = -0.25f * GetPixelScale();
        foreach (var (shape, mat, scissor) in _UIElementRenderQueue)
        {
            Box2 screenScaleScissor = new((scissor.Min + Vector2.One) * 0.5f, (scissor.Max + Vector2.One) * 0.5f);
            var sssMin = screenScaleScissor.Min * _screenSize;
            var sssMax = screenScaleScissor.Max * _screenSize;
            screenScaleScissor = new(sssMin, sssMax);
            if (screenScaleScissor.Min.X <= viewport.Min.X &&
                screenScaleScissor.Max.X >= viewport.Max.X &&
                screenScaleScissor.Min.Y <= viewport.Min.Y &&
                screenScaleScissor.Max.Y >= viewport.Max.Y
            )
            {
                if (scissorActive)
                {
                    GL.Disable(EnableCap.ScissorTest);
                    scissorActive = false;
                }
            }
            else
            {
                if (!scissorActive)
                    GL.Enable(EnableCap.ScissorTest);
                scissorActive = true;
                GL.Scissor(
                    (int)screenScaleScissor.Min.X,
                    (int)screenScaleScissor.Min.Y,
                    (int)screenScaleScissor.Size.X,
                    (int)screenScaleScissor.Size.Y);
            }

            var sh = shape;
            sh.Center += pixelFix;
            _drawMesh.SetShape(sh);
            mat.Use(_drawMesh.GetMeshInfo(), this);
            _drawMesh.Draw();
        }
        _UIElementRenderQueue.Clear();

        GL.Enable(EnableCap.DepthTest);
        if (scissorActive)
            GL.Disable(EnableCap.ScissorTest);

        FrameBuffer.Unuse();
    }

    public override void InputUpdate(InputUpdateArgs input)
    {
        Vector2 NDCPos = input.NormalizedMousePosition * 2 - Vector2.One;
        Vector2 NDCDelta = NDCPos - _previousMousePosition;
        _previousMousePosition = NDCPos;
        Vector2 scrollDelta = input.MouseState.ScrollDelta;

        for (int m = 0; m < (int)MouseButton.Last; m++)
        {
            var butt = (MouseButton)m;
            if (input.MouseState.IsButtonPressed(butt))
            {
                foreach (var root in Scene!.GetDataContainerEnumerable<UIRootUpdate>())
                {
                    var e = new MouseEvent(NDCPos, NDCDelta, default, butt, (MouseButton)(-1), input.MouseState, input.KeyboardState, MouseEventType.PressedButton);
                    root.OnMouse(ref e);
                }
            }
            if (input.MouseState.IsButtonReleased(butt))
            {
                foreach (var root in Scene!.GetDataContainerEnumerable<UIRootUpdate>())
                {
                    var e = new MouseEvent(NDCPos, NDCDelta, default, (MouseButton)(-1), butt, input.MouseState, input.KeyboardState, MouseEventType.ReleasedButton);
                    root.OnMouse(ref e);
                }
            }
        }
        if (NDCDelta != default)
        {
            foreach (var root in Scene!.GetDataContainerEnumerable<UIRootUpdate>())
            {
                var e = new MouseEvent(NDCPos, NDCDelta, default, (MouseButton)(-1), (MouseButton)(-1), input.MouseState, input.KeyboardState, MouseEventType.Move);
                root.OnMouse(ref e);
            }
        }
        if (scrollDelta != default)
        {
            foreach (var root in Scene!.GetDataContainerEnumerable<UIRootUpdate>())
            {
                var e = new MouseEvent(NDCPos, NDCDelta, scrollDelta, (MouseButton)(-1), (MouseButton)(-1), input.MouseState, input.KeyboardState, MouseEventType.Scroll);
                root.OnMouse(ref e);
            }
        }
    }

    public void AddRenderToQueue(Box2 shape, Material material, Box2 scissor) => _UIElementRenderQueue.Add((shape, material, scissor));

    public override Texture2D GetOutputTexture() => Buffer.ColorAttachments[0];
    public override Vector2i GetScreenSize() => _screenSize;
    public Vector2 GetPixelScale() => new Vector2(2, 2) / _screenSize;
    public override void Resize(Vector2i newSize)
    {
        _screenSize = newSize;
        Buffer?.DisposeAndTextures();
        Buffer = new(_screenSize.X, _screenSize.Y);
    }

    protected override void OnDestroyed()
    {
        Buffer.DisposeAndTextures();
        globals.Dispose();
    }

    public override void AddVertexMain(SyntaxTree scope, IndentedTextWriter writer)
    {
        writer.Write(
@"void main()
{
    vertIn_Initialize();
    vertex();
    gl_Position = vertOut.position;
}"
        );
    }
    public override void AddFragmentMain(SyntaxTree scope, IndentedTextWriter writer)
    {
        bool writesAlbedo = false;
        bool writesAlpha = false;
        foreach (var v in scope.pixOut)
        {
            if (v.Name == "Albedo")
                writesAlbedo = true;
            if (v.Name == "Transparency")
                writesAlpha = true;
        }
        writer.Write(
@$"
out vec4 SL_FragColor;
void main()
{{
    pixel();
    SL_FragColor = vec4({(writesAlbedo ? "pixOut.Albedo" : "1.0,1.0,1.0")},{(writesAlpha ? "pixOut.Transparency" : "1.0")});
}}"
        );
    }
}
