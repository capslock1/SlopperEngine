using System;
using System.Drawing;
using System.Reflection;
using SlopperEngine.Core;
using SlopperEngine.SceneObjects;
using SlopperEngine.UI.Base;
using SlopperEngine.UI.Display;
using SlopperEngine.UI.Interaction;
using SlopperEngine.UI.Layout;
using SlopperEngine.UI.Text;

namespace TestProgram;

/// <summary>
/// Shows a demo and lets the user run it.
/// </summary>
public class DemoButton : UIElement
{
    public DemoButton(Spacer uiContainer, MethodInfo createScene, string name, string description)
    {
        Layout.Value = new LinearArrangedLayout
        {
            ChildAlignment = Alignment.Min,
            IsLayoutHorizontal = false,
            StartAtMax = true,
            Padding = UISize.FromPixels(new(5, 2))
        };
        LocalShape = new(0,0,1,0.2f);

        var nameButton = new TextButton($"Run demo \"{name}\"");
        nameButton.OnButtonPressed += b =>
        {
            if(b != OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left)
                return;

            UIElement spacerrr = new();
            uiContainer.UIChildren.Add(spacerrr);
            ColorRectangle rect = new(new(0,0,1,1), default){BlockAnyInput = true, BlockClicks = true, BlockScrolls = true};
            TextBox text = new($"Running demo {name}...", textColor: new(1f,1,1,0))
            {
                LocalShape = new(0.5f,0.5f,0.5f,0.5f),
                Horizontal = Alignment.Middle,
                Scale = 1,
            };
            spacerrr.UIChildren.Add(rect);
            spacerrr.UIChildren.Add(text);

            Children.FirstOfType<ColorFader>()?.Destroy();
            var fadein = new ColorFader(rect, text, true);
            Children.Add(fadein);
            
            var demoScene = (Scene)createScene.Invoke(null, null)!;
            demoScene.OnDestroy += () =>
            {
                Children.FirstOfType<ColorFader>()?.Destroy();
                var fadeout = new ColorFader(rect, text, false);
                Children.Add(fadeout);
                fadeout.OnFinishFade += spacerrr.Destroy;
            };
        };

        UIChildren.Add(nameButton);
        UIChildren.Add(new TextBox(description, textColor: Style.ForegroundStrong){Scale = 1});
    }

    class ColorFader(ColorRectangle rect, TextBox text, bool fadeIn) : SceneObject
    {
        public event Action? OnFinishFade;

        [OnFrameUpdate]
        void FrameUpdate(FrameUpdateArgs args)
        {
            float alpha = text.TextColor.A + (fadeIn ? args.DeltaTime : -2 * args.DeltaTime);
            if(alpha < 0 || alpha > 1)
            {
                alpha = float.Clamp(alpha, 0, 1);
                OnFinishFade?.Invoke();
                Destroy();
            }
            var textCol = text.TextColor;
            textCol.A = alpha;
            text.TextColor = textCol;
            var rectCol = rect.Color;
            rectCol.A = alpha * .5f;
            rect.Color = rectCol;
        }
    }
}