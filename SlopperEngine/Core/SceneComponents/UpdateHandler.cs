
using SlopperEngine.Core.Collections;
using SlopperEngine.SceneObjects;

namespace SlopperEngine.Core.SceneComponents;

/// <summary>
/// Calls every methodHolder that's been added to it, with no respect for order.
/// </summary>
public class UpdateHandler : SceneComponent
{
    /// <summary>
    /// The time that has passed since the instantiation of this UpdateHandler.
    /// </summary>
    public ulong TimeMilliseconds { get; private set; }

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

        FrameUpdater update = new(Scene!, input);
        Scene!.GetDataContainerEnumerable<OnFrameUpdate>().Enumerate(ref update);
    }
    struct FrameUpdater(Scene scene, FrameUpdateArgs update) : IRefEnumerator<OnFrameUpdate>
    {
        Scene _scene = scene;
        FrameUpdateArgs _update = update;
        public void Next(ref OnFrameUpdate value)
        {
            value.Invoke(_update);
            _scene.UpdateRegister();
        }
    }

    public override void InputUpdate(InputUpdateArgs input)
    {
        InputUpdater update = new(Scene!, input);
        Scene!.GetDataContainerEnumerable<OnInputUpdate>().Enumerate(ref update);
    }
    struct InputUpdater(Scene scene, InputUpdateArgs update) : IRefEnumerator<OnInputUpdate>
    {
        Scene _scene = scene;
        InputUpdateArgs _update = update;

        public void Next(ref OnInputUpdate value)
        {
            value.Invoke(_update);
            _scene.UpdateRegister();
        }
    }
}