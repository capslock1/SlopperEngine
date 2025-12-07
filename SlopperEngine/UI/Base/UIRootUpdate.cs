using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using SlopperEngine.Rendering;

namespace SlopperEngine.UI.Base;

/// <summary>
/// Handles to the update functions of the UIElement root.
/// </summary>
public struct UIRootUpdate
{
    /// <summary>
    /// Gets called to update the UIElement's shape.
    /// </summary>
    public Action<Box2, UIRenderer> UpdateShape;
    /// <summary>
    /// Gets called to register renders to the UIRenderer.
    /// </summary>
    public Action<Box2, UIRenderer> AddRender;
    /// <summary>
    /// Gets called when the mouse does anything.
    /// </summary>
    public OnMouseEvent OnMouse;

    /// <summary>
    /// Specific delegate for mouse event updates.
    /// </summary>
    public delegate void OnMouseEvent(ref MouseEvent e);
}
