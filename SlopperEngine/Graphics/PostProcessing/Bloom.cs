using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SlopperEngine.Core;

namespace SlopperEngine.Graphics.PostProcessing;

/// <summary>
/// Adds bloom to a texture.
/// </summary>
public class Bloom : IDisposable
{
    readonly Texture2D[] _mipChain;
    static ComputeShader? _downscale;
    static int _UdownDTS;
    static int _UdownSTS;
    static ComputeShader? _upscale;
    static int _UupDTS;
    static int _UupSTS;
    static int _UupDCont;
    static int _UupSCont;

    /// <summary>
    /// Creates a new instance of the bloom postprocessing effect. Expects the input texture to be Rgba16.
    /// </summary>
    /// <param name="mainTextureSize">The size of the texture to do postprocessing on. Using a different sized image may cause artifacts.</param>
    /// <param name="mipCount">The amount of mips to use. If <0, it will be automatically set to cause the lowest mip to be ~4*4 pixels in size.</param>
    public Bloom(Vector2i mainTextureSize, int mipCount = -1)
    {
        if(mipCount < 0)
        {
            Vector2 textest = mainTextureSize;
            mipCount = 0;
            while(textest.X * textest.Y > 16)
            {
                textest /= 2;
                mipCount++;
            }
        }
        _mipChain = new Texture2D[mipCount];
        for(int i = 0; i<mipCount; i++)
        {
            var pTexSize = mainTextureSize;
            mainTextureSize /= 2;
            if(mainTextureSize.X * 2 < pTexSize.X) mainTextureSize.X++;
            if(mainTextureSize.Y * 2 < pTexSize.Y) mainTextureSize.Y++;
            _mipChain[i] = Texture2D.Create(mainTextureSize.X, mainTextureSize.Y, SizedInternalFormat.Rgba16f);
        }
    }

    /// <summary>
    /// Adds bloom to the target texture. The target texture is expected to be the same size as when the Bloom class was constructed.
    /// </summary>
    public void AddBloom(Texture2D target, float intensity, float sharpness = .25f)
    {
        InitShaders();
        for(int i = 0; i<_mipChain.Length; i++)
        {
            Texture2D pTexture;
            if(i == 0) 
                pTexture = target;
            else pTexture = _mipChain[i-1];

            DownscaleTo(pTexture, _mipChain[i]);
        }
        float contribution = 1f;
        for(int i = _mipChain.Length-1; i>=0; i--)
        {
            Texture2D nTexture;
            if(i == 0)
                nTexture = target;
            else nTexture = _mipChain[i-1];

            contribution *= .5f;
            float lerp = 1-contribution;//1f-(float)i/_mipChain.Length*sharpness;
            UpscaleTo(_mipChain[i], nTexture, i == 0 ? intensity : lerp, i == 0 ? 1-intensity : 1-lerp);
        }
    }

    static void DownscaleTo(Texture2D source, Texture2D destination)
    {
        var ogMinFilter = source.MinificationFilter;
        var ogMaxFilter = source.MagnificationFilter;
        var ogHorWrap = source.HorizontalWrap;
        var ogVertWrap = source.VerticalWrap;
        source.MinificationFilter = TextureMinFilter.Linear;
        source.MagnificationFilter = TextureMagFilter.Linear;
        source.HorizontalWrap = TextureWrapMode.MirroredRepeat;
        source.VerticalWrap = TextureWrapMode.MirroredRepeat;
        source.Use(TextureUnit.Texture0);
        
        destination.UseAsImage(0);
        
        _downscale!.SetUniform(_UdownDTS, new Vector2(1f/destination.Width, 1f/destination.Height));
        _downscale.SetUniform(_UdownSTS, new Vector2(1f/source.Width, 1f/source.Height));
        
        var computeUnits = new Vector2i(destination.Width, destination.Height) / 16;
        if(computeUnits.X * 16 <= destination.Width) computeUnits.X++;
        if(computeUnits.Y * 16 <= destination.Height) computeUnits.Y++;

        _downscale.Dispatch(computeUnits.X, computeUnits.Y, 1);
        GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

        source.MinificationFilter = ogMinFilter;
        source.MagnificationFilter = ogMaxFilter;
        source.HorizontalWrap = ogHorWrap;
        source.VerticalWrap = ogVertWrap;
    }

    static void UpscaleTo(Texture2D source, Texture2D destination, float destcontribution, float sourcecontribution)
    {
        var ogMinFilter = source.MinificationFilter;
        var ogMaxFilter = source.MagnificationFilter;
        var ogHorWrap = source.HorizontalWrap;
        var ogVertWrap = source.VerticalWrap;
        source.MinificationFilter = TextureMinFilter.Linear;
        source.MagnificationFilter = TextureMagFilter.Linear;
        source.HorizontalWrap = TextureWrapMode.MirroredRepeat;
        source.VerticalWrap = TextureWrapMode.MirroredRepeat;
        source.Use(TextureUnit.Texture0);

        destination.UseAsImage(0);

        _upscale!.SetUniform(_UupDTS, new Vector2(1f/destination.Width, 1f/destination.Height));
        _upscale.SetUniform(_UupSTS, new Vector2(1f/source.Width, 1f/source.Height));
        _upscale.SetUniform(_UupDCont, destcontribution);
        _upscale.SetUniform(_UupSCont, sourcecontribution);

        var computeUnits = new Vector2i(destination.Width, destination.Height) / 16;
        if(computeUnits.X * 16 <= destination.Width) computeUnits.X++;
        if(computeUnits.Y * 16 <= destination.Height) computeUnits.Y++;

        _upscale.Dispatch(computeUnits.X, computeUnits.Y, 1);
        GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

        source.MinificationFilter = ogMinFilter;
        source.MagnificationFilter = ogMaxFilter;
        source.HorizontalWrap = ogHorWrap;
        source.VerticalWrap = ogVertWrap;
    }

    static void InitShaders()
    {
        if(_upscale != null) return; //dont even care
        _downscale = ComputeShader.Create(File.ReadAllText(Assets.GetPath("shaders/PostProcessing/BloomDownscale.compute")));
        _UdownDTS = _downscale.GetUniformLocation("DestinationTexelSize");
        _UdownSTS = _downscale.GetUniformLocation("SourceTexelSize");
        _upscale = ComputeShader.Create(File.ReadAllText(Assets.GetPath("shaders/PostProcessing/BloomUpscale.compute")));
        _UupDTS = _upscale.GetUniformLocation("DestinationTexelSize");
        _UupSTS = _upscale.GetUniformLocation("SourceTexelSize");
        _UupDCont = _upscale.GetUniformLocation("DestinationContribution");
        _UupSCont = _upscale.GetUniformLocation("SourceContribution");
    }

    public void Dispose()
    {
        foreach(var tex in _mipChain)
            tex.Dispose();
    }
}