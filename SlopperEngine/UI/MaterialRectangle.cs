using OpenTK.Mathematics;
using SlopperEngine.Graphics;
using SlopperEngine.UI.Base;

namespace SlopperEngine.UI;

/// <summary>
/// Shows a custom material on screen.
/// </summary>
public class MaterialRectangle : UIElement
{
    /// <summary>
    /// The material to display.
    /// </summary>
    public Material? Material = null;

    public MaterialRectangle(Box2 shape, Material material) : base(shape)
    {
        Material = material;
    }
    public MaterialRectangle(Material material) : this(new(0,0,1,1), material){}
    public MaterialRectangle() : this(Material.MissingMaterial){}

    protected override Material? GetMaterial() => Material;
}