using System.Collections.Frozen;
using OpenTK.Mathematics;
using SlopperEngine.Engine;
using SlopperEngine.Engine.Collections;
using StbImageSharp;

namespace SlopperEngine.UI;

/// <summary>
/// Describes a monospace font, made from a texture.
/// </summary>
public class RasterFont
{
    public static readonly RasterFont FourXEight = new("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-+_=`~[]{}/|;:'\".,\\<>? ", new(4,8), "defaultTextures/monospace.png", new(18,3));
    public static readonly RasterFont EightXSixteen = new("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-+_=`~[]{}/|;:'\".,\\<>? ", new(8,16), "defaultTextures/monospace2x.png", new(18,3));

    public readonly Vector2i CharacterSize;
    readonly FrozenDictionary<char, Vector2i> _characterPosSheet;
    readonly Vector2i _missingCharacterPos;
    readonly byte[,] _characterValues;

    private RasterFont(string characters, Vector2i characterSize, string fontImageFilepath, Vector2i missingCharPos)
    {
        CharacterSize = characterSize;
        _missingCharacterPos = missingCharPos * characterSize;
        Dictionary<char, Vector2i> characterPosSheet = new();

        StbImage.stbi_set_flip_vertically_on_load(0);
        ImageResult image = ImageResult.FromStream(File.OpenRead(Assets.GetPath(fontImageFilepath)), ColorComponents.Grey);
        
        Vector2i characterSheetSize = (image.Width / characterSize.X, image.Height / characterSize.Y);

        Vector2i pos = default;
        foreach(char c in characters)
        {
            characterPosSheet.Add(c, pos*characterSize);

            pos.X++;
            if(pos.X >= characterSheetSize.X)
            {
                pos.X = 0;
                pos.Y++;
                if(pos.Y >= characterSheetSize.Y)
                {
                    Console.WriteLine($"problem creating font at '{c}' and onwards, spritesheet was not large enough");
                    break;
                }
            }
        }

        _characterValues = new byte[image.Width, image.Height];
        for(int x = 0; x<image.Width; x++)
            for(int y = 0; y<image.Height; y++)
                _characterValues[x,y] = image.Data[x+y*image.Width];

        _characterPosSheet = characterPosSheet.ToFrozenDictionary();
    }

    /// <summary>
    /// Reads the font's value at a position in the spritesheet.
    /// </summary>
    public byte ReadTexture(Vector2i position)
    {
        return _characterValues[position.X, position.Y];
    }

    /// <summary>
    /// Gets the character's position in the spritesheet.
    /// </summary>
    /// <param name="c">The character to find the position of.</param>
    /// <returns></returns>
    public Vector2i GetCharPosition(char c)
    {
        if(_characterPosSheet.TryGetValue(c, out var res))
            return res;
        
        if(c != ' ' && char.IsWhiteSpace(c))
            return GetCharPosition(' ');
        return _missingCharacterPos;
    }
}