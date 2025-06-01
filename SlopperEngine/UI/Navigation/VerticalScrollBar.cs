using OpenTK.Mathematics;
using SlopperEngine.Core;
using SlopperEngine.UI.Base;

namespace SlopperEngine.UI.Navigation;

public class VerticalScrollBar : Button
{
    /// <summary>
    /// How far up or down the scrollbar is. 0 for on the start, 1 for on the end. 
    /// </summary>
    public float ScrollValue
    {
        get => _scrollValue;
        set
        {
            _scrollValue = float.Clamp(value, 0, 1);
            UpdateBar();
        }
    }
    float _scrollValue = 0;

    /// <summary>
    /// The ratio between the content and the container. If below one, the scrollbar will not be able to move.
    /// </summary>
    public float ContentToContainerRatio
    {
        get => _contentRatio;
        set
        {
            float newRatio = float.Max(value, 1);
            _scrollValue *= _contentRatio / newRatio;
            _scrollValue = float.Min(_scrollValue, 1);
            _contentRatio = newRatio;
            UpdateBar();
        }
    }
    float _contentRatio;

    ColorRectangle _background;
    ColorRectangle _bar;

    public VerticalScrollBar(Color4 backgroundColor, Color4 barColor, float contentRatio, float scrollValue = 0)
    {
        _background = new(new(0, 0, 1, 1), backgroundColor);
        _bar = new(new(0, 0.3f, 1, 0.9f), barColor);
        hiddenUIChildren.Add(_background);
        hiddenUIChildren.Add(_bar);
        _scrollValue = float.Clamp(scrollValue,0,1);
        _contentRatio = float.Max(contentRatio, 1);
        UpdateBar();
    }

    void UpdateBar()
    {
        float barSize = 1 / _contentRatio;
        float barPosition = (1 - barSize) * ScrollValue;
        _bar.LocalShape = new(0, barPosition, 1, barPosition + barSize);
    }

    [OnInputUpdate]
    void OnInput(InputUpdateArgs args)
    {
    }
}