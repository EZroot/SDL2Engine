using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SDL2Engine.Core.Lighting.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Lighting;

public class ShadowPassService : IShadowPassService
{
    private OpenGLHandle m_depthShader;
    int shadowFBO, depthTexture;
    private int shadowWidth = 2048, shadowHeight = 2048;
    private float factorFill = 2f, factorUnit = 4f;

    int depthShader;
    int debugShader;
    private OpenGLHandle debugQuadHandle;

    private List<OpenGLHandle> m_shadowMeshAssets = new();

    public int DepthTexturePtr => depthTexture;
    public void Initialize(int shadowWidth = 2048, int shadowHeight = 2048)
    {
        this.shadowWidth = shadowWidth;
        this.shadowHeight = shadowHeight;

        // Setup shadow framebuffer and depth texture.
        shadowFBO = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFBO);
        depthTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, depthTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent,
            shadowWidth, shadowHeight, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToBorder);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToBorder);
        float[] borderColor = { 1f, 1f, 1f, 1f };
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            TextureTarget.Texture2D, depthTexture, 0);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            throw new Exception("Shadow framebuffer incomplete!");
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        // Create depth shader.
        string depthVert = FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/depth/depth.vert");
        string depthFrag = FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/depth/depth.frag");
        depthShader = GLHelper.CreateShaderProgram(depthVert, depthFrag);

        // Create debug shader for visualizing the shadow map.
        string debugVert = FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/debug/debug.vert");
        string debugFrag = FileHelper.ReadFileContents(PlatformInfo.RESOURCES_FOLDER + "/shaders/3d/debug/debug.frag");
        debugShader = GLHelper.CreateShaderProgram(debugVert, debugFrag);
        
        // Create a fullscreen debug quad.
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
    /// USELESS FOR NOW
    /// </summary>
    /// <param name="asset"></param>
    public void RegisterMesh(OpenGLHandle asset)
    {
        m_shadowMeshAssets.Add(asset);
    }

    //TODO: proper register system so we dont pass asset/model and do opengl render setup each time
    public void RenderShadowPass(OpenGLHandle asset, Matrix4 model, Matrix4 lightView, Matrix4 lightProjection)
    {
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Front);
        GL.Viewport(0, 0, shadowWidth, shadowHeight);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFBO);
        GL.Clear(ClearBufferMask.DepthBufferBit);

        GL.UseProgram(depthShader);
        GL.UniformMatrix4(GL.GetUniformLocation(depthShader, "lightView"), false, ref lightView);
        GL.UniformMatrix4(GL.GetUniformLocation(depthShader, "lightProjection"), false, ref lightProjection);

        GL.Enable(EnableCap.PolygonOffsetFill);
        GL.PolygonOffset(factorFill, factorUnit);

        GL.UniformMatrix4(GL.GetUniformLocation(depthShader, "model"), false, ref model);
        GL.BindVertexArray(asset.Handles.Vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, asset.Handles.VertexCount);

        GL.Disable(EnableCap.PolygonOffsetFill);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void RenderDebugQuad()
    {
        GL.Viewport(0, 0, 1024, 1024);
        GL.UseProgram(debugShader);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, depthTexture);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.None);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)All.Lequal);
        GL.Uniform1(GL.GetUniformLocation(debugShader, "debugTexture"), 0);
        GL.BindVertexArray(debugQuadHandle.Handles.Vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, debugQuadHandle.Handles.VertexCount);

        GL.BindVertexArray(0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GL.UseProgram(0);
        GL.Disable(EnableCap.CullFace);
    }
}