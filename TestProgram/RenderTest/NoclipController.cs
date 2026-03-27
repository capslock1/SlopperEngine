using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SlopperEngine.Core;
using SlopperEngine.SceneObjects;

namespace TestProgram.PerformanceTests;

public class NoclipController : SceneObject
{
    Vector2 _rot;
    float _speedMult = 1;
    Vector3 _input;
    SceneObject3D? _parent3d;

    [OnRegister]
    void OnRegister()
    {
        _parent3d = Parent as SceneObject3D;
        if(_parent3d == null) 
            System.Console.WriteLine($"NoclipController's parent {Parent} was not a SceneObject3D - and hence cannot be controlled.");
    }

    [OnInputUpdate]
    void OnInput(InputUpdateArgs args)
    {
        if(_parent3d == null) return;

        if(!args.MouseState.IsButtonDown(MouseButton.Right))
            return;

        _rot += args.MouseState.Delta * .0025f;
        _rot.X += float.Tau;
        _rot.X %= float.Tau;

        _rot.Y = float.Clamp(_rot.Y, -.49f*float.Pi, .49f*float.Pi);

        _parent3d.LocalRotation = 
        Quaternion.FromAxisAngle(-Vector3.UnitY,_rot.X) 
        * Quaternion.FromAxisAngle(-Vector3.UnitX,_rot.Y);

        float speed = 1;
        if(args.KeyboardState.IsKeyDown(Keys.LeftShift)) speed = 5;
        speed*=_speedMult;
        if(args.KeyboardState.IsKeyDown(Keys.A)) _input.X+=speed;
        if(args.KeyboardState.IsKeyDown(Keys.D)) _input.X-=speed;
        if(args.KeyboardState.IsKeyDown(Keys.W)) _input.Z+=speed;
        if(args.KeyboardState.IsKeyDown(Keys.S)) _input.Z-=speed;
        if(args.KeyboardState.IsKeyDown(Keys.Space)) _input.Y+=speed;
        if(args.KeyboardState.IsKeyDown(Keys.LeftControl)) _input.Y-=speed;

        _speedMult *= 1f + .1f*args.MouseState.ScrollDelta.Y;
    }

    [OnFrameUpdate]
    void OnUpdate(FrameUpdateArgs args)
    {
        if(_parent3d == null) return;

        Matrix3 rotat = Matrix3.CreateFromQuaternion(_parent3d.LocalRotation);
        _parent3d.LocalPosition -= _input.X*args.DeltaTime*rotat.Row0;
        _parent3d.LocalPosition += _input.Y*args.DeltaTime*Vector3.UnitY;
        _parent3d.LocalPosition -= _input.Z*args.DeltaTime*rotat.Row2;
        _input = (0,0,0);
    }
}