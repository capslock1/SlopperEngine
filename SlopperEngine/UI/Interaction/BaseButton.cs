using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SlopperEngine.Core;
using SlopperEngine.Core.SceneData;
using SlopperEngine.SceneObjects;
using SlopperEngine.UI.Base;

namespace SlopperEngine.UI.Interaction;

/// <summary>
/// Base class for buttons. Handles most events.
/// </summary>
public abstract class BaseButton : UIElement
{
    /// <summary>
    /// Whether or not the button can be interacted with.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (value == _enabled)
                return;

            _enabled = value;
            hovered = false;

            if (value)
                OnEnable();
            else OnDisable();
        }
    }
    bool _enabled = true;

    protected bool hovered { get; private set; }
    protected int mouseButtonsHeld { get; private set; }

    SceneDataHandle _optionalInputUpdate;

    //[OnInputUpdate]
    void InputUpdate(InputUpdateArgs args)
    {
        if (!hovered)
            return;

        if (!LastGlobalShape.ContainsInclusive(args.NormalizedMousePosition * 2 - Vector2.One))
        {
            hovered = false;
            mouseButtonsHeld = 0;
            OnMouseExit();
        }
    }

    [OnUnregister]
    void OnUnregister(Scene scene)
    {
        if (_optionalInputUpdate.IsRegistered)
            scene.UnregisterSceneData<OnInputUpdate>(_optionalInputUpdate, default);   
        System.Console.WriteLine("UNregistered optional input");
    }

    static void OptionalInputUpdate(SceneObject button, InputUpdateArgs args)
    {
        if (button is not BaseButton bb)
            return;

        if (bb.LastGlobalShape.ContainsInclusive(args.NormalizedMousePosition * 2 - Vector2.One))
            return;

        if(bb.Scene != null)
            bb.OnUnregister(bb.Scene);
        bb.mouseButtonsHeld = 0;
        bb.OnMouseExit();
    }

    protected override void HandleEvent(ref MouseEvent e)
    {
        if (!Enabled)
        {
            e.Block();
            return;
        }

        if (!_optionalInputUpdate.IsRegistered)
        unsafe
        {
            OnMouseEntry();
            _optionalInputUpdate = Scene!.RegisterSceneData<OnInputUpdate>(new(
                (delegate*<SceneObject, InputUpdateArgs, void>)((Action<SceneObject, InputUpdateArgs>)OptionalInputUpdate).Method.MethodHandle.GetFunctionPointer(),
                this));
            System.Console.WriteLine("registered optional input");
        }
        if (e.Type == MouseEventType.PressedButton)
        {
            mouseButtonsHeld++;
            OnPressed(e.PressedButton);
            e.Use();
            return;
        }
        if (e.Type == MouseEventType.ReleasedButton && mouseButtonsHeld > 0)
        {
            mouseButtonsHeld--;
            OnAnyRelease(e.ReleasedButton);
            e.Use();
            if (mouseButtonsHeld == 0 && Enabled)
                OnAllButtonsReleased();
            return;
        }
        e.Block();
    }

    protected abstract void OnPressed(MouseButton button);
    protected abstract void OnAnyRelease(MouseButton button);
    protected abstract void OnAllButtonsReleased();
    protected abstract void OnMouseEntry();
    protected abstract void OnMouseExit();
    protected abstract void OnEnable();
    protected abstract void OnDisable();
}