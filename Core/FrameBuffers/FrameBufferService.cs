using System;
using OpenTK.Graphics.OpenGL4;
using SDL2Engine.Core.Addressables.Models.Interfaces;
using SDL2Engine.Core.Buffers.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Buffers
{
    public class FrameBufferService : IFrameBufferService
    {
        private OpenGLHandle m_frameBufferQuadHandle;
        private int fbo, colorTex, depthTex;
        private IModelService m_modelService;
        private IGodRayBufferService m_grbService;
        private int _fboWidth, _fboHeight;
        public bool IsInitialized { get; private set; }

        public FrameBufferService(IModelService modelService, IGodRayBufferService godRayBufferService)
        {
            m_modelService = modelService;
            m_grbService = godRayBufferService;
        }

        public void Initialize()
        {
            InitializeFrameBuffer(1920,1080);
            IsInitialized = true;
        }
        
        public void Resize(int newWidth, int newHeight)
        {
            GL.BindTexture(TextureTarget.Texture2D, colorTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                newWidth, newHeight, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            
            GL.BindTexture(TextureTarget.Texture2D, depthTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24,
                newWidth, newHeight, 0,
                PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            
            _fboWidth = newWidth;
            _fboHeight = newHeight;
            
            // (Optional) Re-check status, unbind, etc.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) 
                != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception("Framebuffer not complete!");
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private void InitializeFrameBuffer(int screenWidth, int screenHeight)
        {
            _fboWidth = screenWidth;
            _fboHeight = screenHeight;
            
            m_frameBufferQuadHandle = m_modelService.CreateFullscreenQuad(
                PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/fbo/fbo.vert",
                PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/fbo/fbo.frag");

            fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            colorTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, colorTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                screenWidth, screenHeight, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, colorTex, 0);

            depthTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, depthTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24,
                          screenWidth, screenHeight, 0,
                          PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
                                    TextureTarget.Texture2D, depthTex, 0);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Framebuffer not complete!");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }


        public void BindFramebuffer()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.Viewport(0, 0, _fboWidth, _fboHeight);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void UnbindFramebuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
        }

        // composite pass: combine main scene and god rays
        public void RenderFramebuffer()
        {
            GL.UseProgram(m_frameBufferQuadHandle.Handles.Shader);
            
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, colorTex);
            GL.Uniform1(GL.GetUniformLocation(m_frameBufferQuadHandle.Handles.Shader, "screenTexture"), 0);
            
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, m_grbService.GetTexture());
            GL.Uniform1(GL.GetUniformLocation(m_frameBufferQuadHandle.Handles.Shader, "godrayTexture"), 1);
            
            GL.BindVertexArray(m_frameBufferQuadHandle.Handles.Vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, m_frameBufferQuadHandle.Handles.VertexCount);
            GL.BindVertexArray(0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.UseProgram(0);
        }

        public int GetTexture() => colorTex;
        public int GetDepthTexture() => depthTex;
    }
}
