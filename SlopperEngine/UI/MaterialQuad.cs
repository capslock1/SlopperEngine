using OpenTK.Mathematics;
using SlopperEngine.Rendering;

namespace SlopperEngine.UI;

/// <summary>
/// Shows a custom material on screen.
/// </summary>
public class MaterialQuad : UIElement
{
    /// <summary>
    /// The material to display.
    /// </summary>
    public Material? Material = null;

    public MaterialQuad(Box2 shape, Material material) : base(shape)
    {
        Material = material;
    }
    public MaterialQuad(Material material) : this(new(0,0,1,1), material){}
    public MaterialQuad() : this(Material.MissingMaterial){}

    protected override Material? GetMaterial() => Material;
}