using OpenTK.Mathematics;
using SlopperEngine.Graphics.Renderers;

namespace SlopperEngine.UI;

/// <summary>
/// Handles to the update functions of the UIElement root.
/// </summary>
public struct UIRootUpdate
{
    public Action<Box2, UIRenderer> UpdateShape;
    public Action<UIRenderer> AddRender;
}