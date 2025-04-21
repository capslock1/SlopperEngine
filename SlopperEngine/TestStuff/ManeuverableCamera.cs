using SlopperEngine.Engine;
using SlopperEngine.SceneObjects;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace SlopperEngine.TestStuff;

/// <summary>
/// A camera controlled by WASD+space+lctrl, that can freely fly around the world. Speed is increased or decreased using the scroll wheel.
/// </summary>
public class ManeuverableCamera : Camera
{
    Vector2 _rot;
    float _speedMult = 1;
    Vector3 _input;

    [OnInputUpdate]
    void OnInput(InputUpdateArgs args)
    {
        var updateArgs = args;
        
        if(!updateArgs.MouseState.IsButtonDown(MouseButton.Right))
            return;

        _rot += updateArgs.MouseState.Delta * .0025f;
        _rot.X += MathF.Tau;
        _rot.X %= MathF.Tau;

        _rot.Y = Math.Clamp(_rot.Y, -.49f*MathF.PI, .49f*MathF.PI);

        LocalRotation = 
        Quaternion.FromAxisAngle(-Vector3.UnitY,_rot.X) 
        * Quaternion.FromAxisAngle(-Vector3.UnitX,_rot.Y);

        float speed = 1;
        if(updateArgs.KeyboardState.IsKeyDown(Keys.LeftShift)) speed = 5;
        speed*=_speedMult;
        if(updateArgs.KeyboardState.IsKeyDown(Keys.A)) _input.X+=speed;
        if(updateArgs.KeyboardState.IsKeyDown(Keys.D)) _input.X-=speed;
        if(updateArgs.KeyboardState.IsKeyDown(Keys.W)) _input.Z+=speed;
        if(updateArgs.KeyboardState.IsKeyDown(Keys.S)) _input.Z-=speed;
        if(updateArgs.KeyboardState.IsKeyDown(Keys.Space)) _input.Y+=speed;
        if(updateArgs.KeyboardState.IsKeyDown(Keys.LeftControl)) _input.Y-=speed;

        _speedMult *= 1f + .1f*updateArgs.MouseState.ScrollDelta.Y;
    }

    [OnFrameUpdate]
    void OnUpdate(FrameUpdateArgs args)
    {
        Matrix3 rotat = Matrix3.CreateFromQuaternion(LocalRotation);
        LocalPosition -= _input.X*args.DeltaTime*rotat.Row0;
        LocalPosition += _input.Y*args.DeltaTime*Vector3.UnitY;
        LocalPosition -= _input.Z*args.DeltaTime*rotat.Row2;
        _input = (0,0,0);
    }
}