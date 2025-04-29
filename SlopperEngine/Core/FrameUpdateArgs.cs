using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SlopperEngine.Core;

/// <summary>
/// Arguments for a scene update.
/// </summary>
public class FrameUpdateArgs
{
    /// <summary>
    /// The time that has passed since the last update.
    /// </summary>
    public readonly float DeltaTime;

    public FrameUpdateArgs(float deltaTime)
    {
        DeltaTime = deltaTime;
    }
}