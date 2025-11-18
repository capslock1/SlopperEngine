using System;
using OpenTK.Mathematics;
using SlopperEngine.Rendering;
using SlopperEngine.UI.Layout;

namespace SlopperEngine.UI.Base;

/// <summary>
/// Handles to the update functions of the UIElement root.
/// </summary>
public struct UIRootUpdate
{
    public Action<Box2, UIRenderer> UpdateShape;
    public Action<Box2, UIRenderer> AddRender;
    public OnMouseEvent OnMouse;

    public delegate void OnMouseEvent(ref MouseEvent e);
}
