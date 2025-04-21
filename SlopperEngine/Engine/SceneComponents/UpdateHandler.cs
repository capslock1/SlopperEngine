
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Engine.SceneComponents;

/// <summary>
/// Calls every methodHolder that's been added to it, with no respect for order.
/// </summary>
public class UpdateHandler : SceneComponent
{    
    /// <summary>
    /// The time that has passed since the instantiation of this UpdateHandler.
    /// </summary>
    public ulong TimeMilliseconds {get; private set;}

    private float _remainder;

    /// <summary>
    /// Updates all objects in the scene with OnFrameUpdate.
    /// </summary>
    /// <param name="input">The input parameter to the EngineMethod.</param>
    public override void FrameUpdate(FrameUpdateArgs input)
    {
        //accurate millisecond timer (minimal rounding error).
        _remainder += input.DeltaTime * 1000;
        var millis = (ulong)MathF.Floor(_remainder);
        _remainder -= millis;
        TimeMilliseconds += millis;

        foreach(var u in Scene!.GetDataContainerEnumerable<OnFrameUpdate>())
        {
            u.Invoke(input);
            Scene?.UpdateRegister();
        }
    }

    public override void InputUpdate(InputUpdateArgs input)
    {
        foreach(var u in Scene!.GetDataContainerEnumerable<OnInputUpdate>())
        {
            u.Invoke(input);
            Scene?.UpdateRegister();
        }
    }
}