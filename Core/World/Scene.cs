using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SDL2;
using SDL2Engine.Core.Buffers.Interfaces;
using SDL2Engine.Core.Cameras;
using SDL2Engine.Core.Lighting;
using SDL2Engine.Core.Lighting.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.World;
public class Scene
{
    private List<GameObject3D> SceneObjects = new();
    private Light m_directionalLight;
    private IFrameBufferService m_frameBufferService;
    private IGodRayBufferService m_godrayBufferService;
    private IShadowPassService m_shadowPassService;
    private CameraGL3D m_camera;
    
    public Light DirectionalLight => m_directionalLight;
    
    public Scene(IFrameBufferService fboService, IShadowPassService shadowPassService, IGodRayBufferService godRayBufferService, CameraGL3D mainCamera)
    {
        m_frameBufferService = fboService;
        m_shadowPassService = shadowPassService;
        m_camera = mainCamera;
        m_godrayBufferService = godRayBufferService;
        
        m_frameBufferService.Initialize();

        // cache shader uniforms, setup/register to shadowmap, etc?
        m_directionalLight = new Light(LightType.Directional, 50, 1, 100);
        m_directionalLight.Update(new Vector3(0, -1, 0), Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(45),MathHelper.DegreesToRadians(180),0), 0);

        m_shadowPassService.Initialize();
    }

    public void AddGameObject(GameObject3D gameObject3D)
    {
        SceneObjects.Add(gameObject3D);
        if (gameObject3D.CastShadows)
        {
            m_shadowPassService.RegisterMesh(gameObject3D.Mesh, gameObject3D.ModelMatrix);
        }
    }

    public void RemoveGameObject(GameObject3D gameObject3D)
    {
        if (gameObject3D.CastShadows)
        {
            m_shadowPassService.UnregisterMesh(gameObject3D.Mesh, gameObject3D.ModelMatrix);
        }
        SceneObjects.Remove(gameObject3D);
    }

    public void Render()
    {
        m_frameBufferService.Resize(PlatformInfo.WindowSize.X, PlatformInfo.WindowSize.Y);
        // // shadow pass
        // m_shadowPassService.RenderShadowPass(m_directionalLight.LightView, m_directionalLight.LightProjection);
        // main render pass
        m_frameBufferService.BindFramebuffer();
        foreach (var obj in SceneObjects)
        {
            if(obj == null) continue;
            
            m_shadowPassService.UpdateMeshModel(obj.Mesh, obj.ModelMatrix);
            
            obj.Shader.Bind();
            var camView = m_camera.View;
            var camProj = m_camera.Projection;
            var modelMat = obj.ModelMatrix;
            var lightSpaceMatrix = m_directionalLight.LightProjection * m_directionalLight.LightView;
            var textureLoc = obj.TextureData.Texture;
            var lightDir = m_directionalLight.LightDirection;
            var lightColor = obj.Color;
            var ambientColor = obj.AmbientColor;
            
            var shadowMapPtr = m_shadowPassService.DepthTexturePtr;
            
            int loc = GL.GetUniformLocation(obj.Shader.ProgramId, "lightDir");
            if (loc != -1) GL.Uniform3(loc, ref lightDir);
        
            loc = GL.GetUniformLocation(obj.Shader.ProgramId, "lightColor");
            if (loc != -1) GL.Uniform3(loc, ref lightColor);
        
            loc = GL.GetUniformLocation(obj.Shader.ProgramId, "ambientColor");
            if (loc != -1) GL.Uniform3(loc, ref ambientColor);
            
            loc = GL.GetUniformLocation(obj.Shader.ProgramId, "model");
            if (loc != -1) GL.UniformMatrix4(loc, false, ref modelMat);
        
            loc = GL.GetUniformLocation(obj.Shader.ProgramId, "view");
            if (loc != -1) GL.UniformMatrix4(loc, false, ref camView);
        
            loc = GL.GetUniformLocation(obj.Shader.ProgramId, "projection");
            if (loc != -1) GL.UniformMatrix4(loc, false, ref camProj);
            
            loc = GL.GetUniformLocation(obj.Shader.ProgramId, "lightSpaceMatrix");
            if (loc != -1) GL.UniformMatrix4(loc, false, ref lightSpaceMatrix);
        
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, (int)textureLoc);
            loc = GL.GetUniformLocation(obj.Shader.ProgramId, "diffuseTexture");
            if (loc != -1) GL.Uniform1(loc, 0);
        
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, shadowMapPtr);
            loc = GL.GetUniformLocation(obj.Shader.ProgramId, "shadowMap");
            if (loc != -1) GL.Uniform1(loc, 1);
        
            GL.BindVertexArray(obj.Mesh.Vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, obj.Mesh.VertexCount);
        
            GL.BindVertexArray(0);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            
            obj.Shader.UnBind();
        }
        
        // m_shadowPassService.RenderDebugQuad(false,1,100);
        m_frameBufferService.UnbindFramebuffer();
        // post process pass
        // m_godrayBufferService.BindFramebuffer();
        // foreach (var obj in SceneObjects)
        // {
        //     obj.Shader.Bind();
        //     var camView = m_camera.View;
        //     var camProj = m_camera.Projection;
        //     var modelMat = obj.ModelMatrix;
        //     var lightSpaceMatrix = m_directionalLight.LightProjection * m_directionalLight.LightView;
        //     var textureLoc = obj.TextureData.Texture;
        //     var lightDir = m_directionalLight.LightDirection;
        //     var lightColor = obj.Color;
        //     var ambientColor = obj.AmbientColor;
        //     
        //     var shadowMapPtr = m_shadowPassService.DepthTexturePtr;
        //     
        //     int loc = GL.GetUniformLocation(obj.Shader.ProgramId, "lightDir");
        //     if (loc != -1) GL.Uniform3(loc, ref lightDir);
        //
        //     loc = GL.GetUniformLocation(obj.Shader.ProgramId, "lightColor");
        //     if (loc != -1) GL.Uniform3(loc, ref lightColor);
        //
        //     loc = GL.GetUniformLocation(obj.Shader.ProgramId, "ambientColor");
        //     if (loc != -1) GL.Uniform3(loc, ref ambientColor);
        //     
        //     loc = GL.GetUniformLocation(obj.Shader.ProgramId, "model");
        //     if (loc != -1) GL.UniformMatrix4(loc, false, ref modelMat);
        //
        //     loc = GL.GetUniformLocation(obj.Shader.ProgramId, "view");
        //     if (loc != -1) GL.UniformMatrix4(loc, false, ref camView);
        //
        //     loc = GL.GetUniformLocation(obj.Shader.ProgramId, "projection");
        //     if (loc != -1) GL.UniformMatrix4(loc, false, ref camProj);
        //     
        //     loc = GL.GetUniformLocation(obj.Shader.ProgramId, "lightSpaceMatrix");
        //     if (loc != -1) GL.UniformMatrix4(loc, false, ref lightSpaceMatrix);
        //
        //     GL.ActiveTexture(TextureUnit.Texture0);
        //     GL.BindTexture(TextureTarget.Texture2D, (int)textureLoc);
        //     loc = GL.GetUniformLocation(obj.Shader.ProgramId, "diffuseTexture");
        //     if (loc != -1) GL.Uniform1(loc, 0);
        //
        //     GL.ActiveTexture(TextureUnit.Texture1);
        //     GL.BindTexture(TextureTarget.Texture2D, shadowMapPtr);
        //     loc = GL.GetUniformLocation(obj.Shader.ProgramId, "shadowMap");
        //     if (loc != -1) GL.Uniform1(loc, 1);
        //
        //     GL.BindVertexArray(obj.Mesh.Vao);
        //     GL.DrawArrays(PrimitiveType.Triangles, 0, obj.Mesh.VertexCount);
        //
        //     GL.BindVertexArray(0);
        //     GL.ActiveTexture(TextureUnit.Texture1);
        //     GL.BindTexture(TextureTarget.Texture2D, 0);
        //     GL.ActiveTexture(TextureUnit.Texture0);
        //     GL.BindTexture(TextureTarget.Texture2D, 0);
        //     
        //     obj.Shader.UnBind();
        // }
        // m_godrayBufferService.UnbindFramebuffer();
        //     
        // // process god rays
        // m_godrayBufferService.ProcessGodRays(m_camera, m_directionalLight, m_frameBufferService.GetDepthTexture());
        // // grbService.RenderDebug(); // visualize god rays 
        //
        // render frame buffer to camera
        m_frameBufferService.RenderFramebuffer();
        
    }
}