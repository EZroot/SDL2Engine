using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SDL2Engine.Core.Addressables.Models.Interfaces;
using SDL2Engine.Core.Buffers.Interfaces;
using SDL2Engine.Core.Cameras;
using SDL2Engine.Core.Lighting;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Buffers
{
    public class GodRayBufferService : IGodRayBufferService
    {
        private OpenGLHandle m_frameBufferQuadHandle; // Quad for god ray processing.
        private OpenGLHandle m_debugQuadHandle;         // Debug quad.
        
        // FBO and texture for god ray processing.
        private int godRayFbo, godRayTex;
        
        // FBO and texture for world geometry (bright-pass source).
        private int worldFbo, worldTex;
        
        private IModelService m_modelService;

        public GodRayBufferService(IModelService modelService)
        {
            m_modelService = modelService;
            InitializeFrameBuffers(1920, 1080);
            // Debug quad already created in InitializeFrameBuffers.
        }
        
        private void InitializeFrameBuffers(int screenWidth, int screenHeight)
        {
            // Create debug quad (pass-through shader for viewing).
            m_debugQuadHandle = m_modelService.CreateFullscreenQuad(
                PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/debug/debug.vert",
                PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/debug/debug.frag");
            
            // Create quad for processing god rays.
            m_frameBufferQuadHandle = m_modelService.CreateFullscreenQuad(
                PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/godrays/godray.vert",
                PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/godrays/godray.frag");

            // --- Create world FBO (for rendering world geometry/bright pass) ---
            worldFbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, worldFbo);

            worldTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, worldTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                screenWidth, screenHeight, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, worldTex, 0);

            // Optional depth buffer for world FBO.
            int depthRboWorld = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRboWorld);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, screenWidth, screenHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
                RenderbufferTarget.Renderbuffer, depthRboWorld);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("World framebuffer not complete!");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // --- Create god ray FBO (for processing god rays) ---
            godRayFbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, godRayFbo);

            godRayTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, godRayTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                screenWidth, screenHeight, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, godRayTex, 0);

            // Optional depth buffer for god ray FBO.
            int depthRboGodRay = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRboGodRay);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, screenWidth, screenHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
                RenderbufferTarget.Renderbuffer, depthRboGodRay);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("GodRay framebuffer not complete!");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        // Bind the world FBO for rendering your scene geometry.
        public void BindFramebuffer(int screenWidth, int screenHeight)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, worldFbo);
            GL.Viewport(0, 0, screenWidth, screenHeight);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        // Unbind the world FBO.
        public void UnbindFramebuffer()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
        }

        // Process god rays by using the world texture as the bright-pass input.
        // The depthTexture parameter remains the main scene depth texture.
        public void ProcessGodRays(CameraGL3D cam, Light light, int depthTexture)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, godRayFbo);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            var shader = m_frameBufferQuadHandle.Handles.Shader;
            GL.UseProgram(shader);

            // Bind the world texture (from worldFbo) to texture unit 0.
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, worldTex);
            GL.Uniform1(GL.GetUniformLocation(shader, "godrayTex"), 0);

            // Bind main scene depth texture to unit 1.
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);
            GL.Uniform1(GL.GetUniformLocation(shader, "depthTex"), 1);

            // Set camera matrices.
            var camView = cam.View;
            var camProj = cam.Projection;
            GL.UniformMatrix4(GL.GetUniformLocation(shader, "view"), false, ref camView);
            GL.UniformMatrix4(GL.GetUniformLocation(shader, "projection"), false, ref camProj);
            Matrix4 invProjection = Matrix4.Invert(cam.Projection);
            Matrix4 invView = Matrix4.Invert(cam.View);
            GL.UniformMatrix4(GL.GetUniformLocation(shader, "invProjection"), false, ref invProjection);
            GL.UniformMatrix4(GL.GetUniformLocation(shader, "invView"), false, ref invView);

            GL.Uniform3(GL.GetUniformLocation(shader, "cameraPos"), cam.Position);
            GL.Uniform3(GL.GetUniformLocation(shader, "lightPos"), light.LightPosition);
            GL.Uniform3(GL.GetUniformLocation(shader, "lightColor"), new Vector3(1,1,1));

            GL.Uniform1(GL.GetUniformLocation(shader, "numSamples"), 512);
            GL.Uniform1(GL.GetUniformLocation(shader, "density"), 0.45f);
            GL.Uniform1(GL.GetUniformLocation(shader, "decay"), 0.9f);
            GL.Uniform1(GL.GetUniformLocation(shader, "weight"), 0.2f);
            GL.Uniform1(GL.GetUniformLocation(shader, "exposure"), 0.5f);

            GL.BindVertexArray(m_frameBufferQuadHandle.Handles.Vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, m_frameBufferQuadHandle.Handles.VertexCount);
            GL.BindVertexArray(0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.UseProgram(0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public int GetTexture() => godRayTex;

        public void RenderDebug()
        {
            GL.UseProgram(m_debugQuadHandle.Handles.Shader);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, godRayTex);
            GL.Uniform1(GL.GetUniformLocation(m_debugQuadHandle.Handles.Shader, "screenTexture"), 0);
            GL.BindVertexArray(m_debugQuadHandle.Handles.Vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, m_debugQuadHandle.Handles.VertexCount);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }
    }
}
