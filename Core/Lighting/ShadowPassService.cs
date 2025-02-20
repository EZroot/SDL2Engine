using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SDL2Engine.Core.Geometry;
using SDL2Engine.Core.Lighting.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Lighting
{
    public class ShadowPassService : IShadowPassService
    {
        int shadowFBO, depthTexture;
        int shadowWidth = 2048, shadowHeight = 2048;
        float factorFill = 1.1f, factorUnit = 4f;

        int depthShader;
        int debugShader;
        OpenGLHandle debugQuadHandle;

        // Cache uniform locations to avoid lookups every draw call.
        int locLightView, locLightProjection, locModel;

        // Registered meshes with their model matrices.
        readonly List<(OpenGLHandle Asset, Matrix4 Model)> m_shadowAssetsOld = new();
        readonly List<(Mesh Asset, Matrix4 Model)> m_shadowAssetsMesh = new();

        public int DepthTexturePtr => depthTexture;

        public void Initialize(int shadowWidth = 2048, int shadowHeight = 2048)
        {
            this.shadowWidth = shadowWidth;
            this.shadowHeight = shadowHeight;

            // Create and bind shadow FBO.
            shadowFBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFBO);

            // Create depth texture.
            depthTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent,
                shadowWidth, shadowHeight, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            // Set texture parameters.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Nearest);

            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            // Attach texture to FBO.
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D, depthTexture, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Shadow framebuffer incomplete!");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // Create depth shader and cache its uniforms.
            string depthVert =
                FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/depth/depth.vert");
            string depthFrag =
                FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/depth/depth.frag");
            depthShader = GLHelper.CreateShaderProgram(depthVert, depthFrag);
            locLightView = GL.GetUniformLocation(depthShader, "lightView");
            locLightProjection = GL.GetUniformLocation(depthShader, "lightProjection");
            locModel = GL.GetUniformLocation(depthShader, "model");

            // Create debug shader.
            string debugVert =
                FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/debug/debug.vert");
            string debugFrag =
                FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/debug/debug.frag");
            debugShader = GLHelper.CreateShaderProgram(debugVert, debugFrag);

            // Create fullscreen debug quad.
            float[] quadVertices =
            {
                -1f, 1f, 0f, 1f,
                -1f, 0f, 0f, 0f,
                0f, 0f, 1f, 0f,
                -1f, 1f, 0f, 1f,
                0f, 0f, 1f, 0f,
                0f, 1f, 1f, 1f,
            };
            int quadVao = GL.GenVertexArray();
            int quadVbo = GL.GenBuffer();
            GL.BindVertexArray(quadVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices,
                BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.BindVertexArray(0);

            debugQuadHandle = new OpenGLHandle(new OpenGLMandatoryHandles(quadVao, quadVbo, 0, debugShader, 6));
        }
        
        /// <summary>
        /// Register a mesh to our shadow pass and its initial model matrix
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="model"></param>
        public void RegisterMesh(Mesh asset, Matrix4 model)
        {
            m_shadowAssetsMesh.Add((asset, model));
        }
        
        public void UnregisterMesh(Mesh asset, Matrix4 model)
        {
            m_shadowAssetsMesh.Remove((asset, model));
        }

        /// <summary>
        /// Update a mesh assets model matrix
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="newModel"></param>
        public void UpdateMeshModel(Mesh asset, Matrix4 newModel)
        {
            for (int i = 0; i < m_shadowAssetsMesh.Count; i++)
            {
                if (m_shadowAssetsMesh[i].Asset.Equals(asset))
                {
                    m_shadowAssetsMesh[i] = (asset, newModel);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Render a shadow pass on all registered meshes based on a directional light
        /// </summary>
        /// <param name="lightView"></param>
        /// <param name="lightProjection"></param>
        public void RenderShadowPass(Matrix4 lightView, Matrix4 lightProjection)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.CullFace);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFBO);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            GL.UseProgram(depthShader);
            GL.UniformMatrix4(locLightView, false, ref lightView);
            GL.UniformMatrix4(locLightProjection, false, ref lightProjection);

            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(factorFill, factorUnit);

            foreach (var (asset, model) in m_shadowAssetsMesh)
            {
                var assetModel = model;
                GL.UniformMatrix4(locModel, false, ref assetModel);
                GL.BindVertexArray(asset.Vao);
                GL.DrawArrays(PrimitiveType.Triangles, 0, asset.VertexCount);
            }

            GL.Disable(EnableCap.PolygonOffsetFill);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void RenderDebugQuad(bool showRawDepth, float nearPlane, float farPlane)
        {
            GL.UseProgram(debugShader);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);

            // Ensure we're sampling raw depth data.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode,
                (int)TextureCompareMode.None);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)All.Lequal);

            // Update shader uniforms.
            int debugTextureLoc = GL.GetUniformLocation(debugShader, "debugTexture");
            int showRawDepthLoc = GL.GetUniformLocation(debugShader, "showRawDepth");
            int nearPlaneLoc = GL.GetUniformLocation(debugShader, "nearPlane");
            int farPlaneLoc = GL.GetUniformLocation(debugShader, "farPlane");
            GL.Uniform1(debugTextureLoc, 0);
            GL.Uniform1(showRawDepthLoc, showRawDepth ? 1 : 0);
            GL.Uniform1(nearPlaneLoc, nearPlane);
            GL.Uniform1(farPlaneLoc, farPlane);

            GL.BindVertexArray(debugQuadHandle.Handles.Vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, debugQuadHandle.Handles.VertexCount);
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.UseProgram(0);
            GL.Disable(EnableCap.CullFace);
        }
        
        #region (OLD) Deprecated - Remove this shit later
        /// <summary>
        /// Register a mesh to our shadow pass and its initial model matrix
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="model"></param>
        public void RegisterMesh(OpenGLHandle asset, Matrix4 model)
        {
            m_shadowAssetsOld.Add((asset, model));
        }

        /// <summary>
        /// Update a mesh assets model matrix
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="newModel"></param>
        public void UpdateMeshModel(OpenGLHandle asset, Matrix4 newModel)
        {
            for (int i = 0; i < m_shadowAssetsOld.Count; i++)
            {
                if (m_shadowAssetsOld[i].Asset.Equals(asset))
                {
                    m_shadowAssetsOld[i] = (asset, newModel);
                    break;
                }
            }
        }

        /// <summary>
        /// Render a shadow pass on all registered meshes based on a directional light
        /// </summary>
        /// <param name="lightView"></param>
        /// <param name="lightProjection"></param>
        public void RenderShadowPassDeprecated(Matrix4 lightView, Matrix4 lightProjection)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.CullFace);
            // GL.CullFace(CullFaceMode.Front);
            GL.Viewport(0, 0, shadowWidth, shadowHeight);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFBO);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            GL.UseProgram(depthShader);
            GL.UniformMatrix4(locLightView, false, ref lightView);
            GL.UniformMatrix4(locLightProjection, false, ref lightProjection);

            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(factorFill, factorUnit);

            foreach (var (asset, model) in m_shadowAssetsOld)
            {
                var assetModel = model;
                GL.UniformMatrix4(locModel, false, ref assetModel);
                GL.BindVertexArray(asset.Handles.Vao);
                GL.DrawArrays(PrimitiveType.Triangles, 0, asset.Handles.VertexCount);
            }

            GL.Disable(EnableCap.PolygonOffsetFill);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        /// <summary>
        /// Render depth shader debug quad
        /// </summary>
        public void RenderDebugQuadOld(bool showRawDepth, float nearPlane, float farPlane)
        {
            GL.Viewport(0, 0, 1024, 1024);
            GL.UseProgram(debugShader);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, depthTexture);

            // Ensure we're sampling raw depth data.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode,
                (int)TextureCompareMode.None);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)All.Lequal);

            // Update shader uniforms.
            int debugTextureLoc = GL.GetUniformLocation(debugShader, "debugTexture");
            int showRawDepthLoc = GL.GetUniformLocation(debugShader, "showRawDepth");
            int nearPlaneLoc = GL.GetUniformLocation(debugShader, "nearPlane");
            int farPlaneLoc = GL.GetUniformLocation(debugShader, "farPlane");
            GL.Uniform1(debugTextureLoc, 0);
            GL.Uniform1(showRawDepthLoc, showRawDepth ? 1 : 0);
            GL.Uniform1(nearPlaneLoc, nearPlane);
            GL.Uniform1(farPlaneLoc, farPlane);

            GL.BindVertexArray(debugQuadHandle.Handles.Vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, debugQuadHandle.Handles.VertexCount);
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.UseProgram(0);
            GL.Disable(EnableCap.CullFace);
        }
        #endregion
    }
}