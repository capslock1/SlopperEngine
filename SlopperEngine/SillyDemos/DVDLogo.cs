using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.UI.Base;

namespace SlopperEngine.SillyDemos;

/// <summary>
/// Bounces around in the container its in.
/// </summary>
public class DVDLogo : UIElement
{
    public event Action? OnBounce;

    Vector2 _velocity;
    Vector2 _halfSize;
    Vector2 _position;

    public DVDLogo(Vector2 size, float speed = .1f) : base()
    {
        _position = new(.5f,.5f);
        _halfSize = size * .5f;
        _velocity = new(speed, MathF.Sqrt(2)*speed);
        LocalShape = new(_position-_halfSize, _position+_halfSize);
    }

    // simple overlap detection with the boundaries of its container.
    [OnFrameUpdate] void Frame(FrameUpdateArgs args)
    {
        _position += _velocity * args.DeltaTime;
        LocalShape = new(_position-_halfSize, _position+_halfSize);

        if((LocalShape.Max.X > 1 && _velocity.X > 0) ||
            (LocalShape.Min.X < 0 && _velocity.X < 0))
        {
            _velocity.X = -_velocity.X;
            OnBounce?.Invoke();
        }

        if((LocalShape.Max.Y > 1 && _velocity.Y > 0) ||
            (LocalShape.Min.Y < 0 && _velocity.Y < 0))
        {
            _velocity.Y = -_velocity.Y;
            OnBounce?.Invoke();
        }
    }
}