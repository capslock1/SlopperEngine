using System;
using OpenTK.Mathematics;
using SlopperEngine.Graphics.GPUResources.Textures;

namespace SlopperEngine.UI.Text;

/// <summary>
/// Converts strings to textures using a raster font image.
/// </summary>
public static class RasterFontWriter
{
    /// <summary>
    /// Writes a string to a texture. May or may not replace the texture with a new instance, if the size is inadequate.
    /// </summary>
    /// <param name="text">The text to write.</param>
    /// <param name="font">The font to use.</param>
    /// <param name="maxWidthPixels">The maximum width of the result texture in pixels. May be slightly exceeded.</param>
    public static Texture2D? WriteToTexture2D(ReadOnlySpan<char> text, RasterFont font, uint maxWidthPixels = uint.MaxValue)
    {
        if(text.Length == 0) return null;

        int charWidth = font.CharacterSize.X;
        int charHeight = font.CharacterSize.Y;
        int currentLineWidth = 0;
        int totalWidth = 0;
        int totalHeight = charHeight;
        for(int i = 0; i<text.Length; i++)
        {
            if(text[i] != '\n')
            {
                currentLineWidth += charWidth;
                if(currentLineWidth >= maxWidthPixels) 
                {
                    totalWidth = currentLineWidth;
                    currentLineWidth = 0;
                    totalHeight += charHeight;
                    continue;
                }
            }
            else
            {
                totalWidth = int.Max(totalWidth, currentLineWidth);
                currentLineWidth = 0;
                totalHeight += charHeight;
            }
        }

        totalWidth = int.Max(int.Max(charWidth, totalWidth), currentLineWidth);

        byte[] res = new byte[totalHeight*totalWidth];
        Vector2i texPos = default;
        bool justNewLined = false;
        for(int i = 0; i<text.Length; i++)
        {
            var ch = text[i];
            if(ch != '\n')
            {
                justNewLined = false;
                Vector2i charPos = font.GetCharPosition(ch);
                for(int x = 0; x<charWidth; x++)
                for(int y = 0; y<charHeight; y++)
                {
                    var off = new Vector2i(x,y);
                    if(font.ReadTexture(charPos + off) > 127)
                    {
                        var texpospos = texPos + off;
                        res[texpospos.X + (totalHeight - texpospos.Y - 1) * totalWidth] = 255;
                    }
                }
                texPos.X += charWidth;
                if(texPos.X >= totalWidth)
                {
                    justNewLined = true;
                    texPos.Y += charHeight;
                    texPos.X = 0;
                }
            }
            else if(!justNewLined)
            {
                texPos.X = 0;
                texPos.Y += charHeight;
            }
        }

        return Texture2D.Create(totalWidth, totalHeight, OpenTK.Graphics.OpenGL4.SizedInternalFormat.Rgba8, OpenTK.Graphics.OpenGL4.PixelFormat.Red, res);
    }
}