using System.Collections.ObjectModel;
using OpenTK.Graphics.OpenGL4;
using SlopperEngine.Graphics.GPUResources.Textures;

namespace SlopperEngine.Graphics.GPUResources;

/// <summary>
/// A container for textures to draw to using a DrawShader.
/// </summary>
public class FrameBuffer : GPUResource
{
    public readonly int FBO;
    public readonly int RBO;
    public readonly int Width;
    public readonly int Height;
    public readonly ReadOnlyCollection<Texture2D> ColorAttachments;
    public FrameBuffer(int width, int height, int colorAttachmentCount = 1)
    {
        if(colorAttachmentCount < 1 || colorAttachmentCount > 8)
            throw new Exception("Attempted to create FrameBuffer with "+colorAttachmentCount+" color attachments, needs at least 1 and at most 8");
        
        FBO = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

        this.Width = width;
        this.Height = height;
        List<Texture2D> textures = new();
        for(int i = 0; i<colorAttachmentCount; i++)
        {
            var tex = Texture2D.Create(width, height, SizedInternalFormat.Rgba16f);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, tex.Handle, 0);
            textures.Add(tex);
        }
        ColorAttachments = new(textures);

        RBO = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RBO);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, RBO);

        var error = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if(error != FramebufferErrorCode.FramebufferComplete)
            Console.WriteLine("WHOOPS! frame buffer did NOT complete: "+error);
    }

    /// <summary>
    /// Makes the buffer active, resulting in all following drawcalls writing to it.
    /// </summary>
    public void Use()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        GL.Viewport(0, 0, Width, Height);
    }

    public void DisposeAndTextures()
    {
        Dispose();
        foreach(var j in ColorAttachments)
            j.Dispose();
    }


    /// <summary>
    /// Uses the default framebuffer (owned by the current NativeWindow).
    /// </summary>
    public static void Unuse()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    protected override ResourceData GetResourceData() => new UBOResourceData(){FBO = this.FBO, RBO = this.RBO};

    protected override IGPUResourceOrigin GetOrigin() => new FrameBufferOrigin(Width, Height, ColorAttachments.Count);
    protected class FrameBufferOrigin(int width, int height, int colorAttachmentCount = 1) : IGPUResourceOrigin
    {
        int width = width;
        int height = height;
        int colorAttachmentCount = colorAttachmentCount;

        public GPUResource CreateResource()
        {
            return new FrameBuffer(width, height, colorAttachmentCount);
        }
    }

    protected class UBOResourceData : ResourceData
    {
        public required int FBO;
        public int RBO;
        public override void Clear()
        {
            GL.DeleteFramebuffer(FBO);
            GL.DeleteRenderbuffer(RBO);
        }
    }
}