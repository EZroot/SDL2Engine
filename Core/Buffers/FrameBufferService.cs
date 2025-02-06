using System.Globalization;
using OpenTK.Graphics.OpenGL4;
using SDL2Engine.Core.Addressables.Models.Interfaces;
using SDL2Engine.Core.Buffers.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Buffers;

public class FrameBufferService : IFrameBufferService
{
    private OpenGLHandle m_frameBufferQuadHandle;
    private int fbo, colorTex;
    private IModelService m_modelService;

    public FrameBufferService(IModelService modelService)
    {
        m_modelService = modelService;
        InitializeFrameBuffer(1920, 1080);
    }
    
    private void InitializeFrameBuffer(int screenWidth, int screenHeight)
    {
        m_frameBufferQuadHandle = m_modelService.CreateFullscreenQuad(
            PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/fbo/fbo.vert",
            PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/fbo/fbo.frag");

        fbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

        colorTex = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, colorTex);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, screenWidth, screenHeight, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, colorTex, 0);

        int depthRbo = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRbo);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, screenWidth,
            screenHeight);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer, depthRbo);

        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            throw new Exception("Framebuffer not complete!");
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void BindFramebuffer(int screenWidth, int screenHeight)
    {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Back);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        GL.Viewport(0, 0, screenWidth, screenHeight);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    public void UnbindFramebuffer()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);
    }

    public void RenderFramebuffer()
    {
        GL.UseProgram(m_frameBufferQuadHandle.Handles.Shader);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, colorTex);
        GL.Uniform1(GL.GetUniformLocation(m_frameBufferQuadHandle.Handles.Shader, "screenTexture"), 0);

        GL.BindVertexArray(m_frameBufferQuadHandle.Handles.Vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, m_frameBufferQuadHandle.Handles.VertexCount);

        GL.BindVertexArray(0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GL.UseProgram(0);
    }
}