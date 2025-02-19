using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SDL2Engine.Core.Buffers.Interfaces;
using SDL2Engine.Core.Cameras;
using SDL2Engine.Core.Lighting;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.World;

public class Scene
{
    private List<GameObject3D> SceneObjects = new();
    private Light m_directionalLight;
    private IFrameBufferService m_frameBufferService;
    private CameraGL3D m_camera;
    public Scene(IFrameBufferService fboService, CameraGL3D mainCamera)
    {
        m_frameBufferService = fboService;
        m_camera = mainCamera;
    }

    public void AddGameObject(GameObject3D gameObject3D)
    {
        SceneObjects.Add(gameObject3D);
    }

    public void RemoveGameObject(GameObject3D gameObject3D)
    {
        SceneObjects.Remove(gameObject3D);
    }

    public void Start()
    {
        // cache shader uniforms, setup/register to shadowmap, etc?
        m_directionalLight = new Light(LightType.Directional, 60, 1, 100);
    }

    public void Render()
    {
        // shadow pass
        m_frameBufferService.BindFramebuffer(1920,1080);
        // main render pass
        Debug.Log($"SceneObj {SceneObjects.Count}");
        foreach (var obj in SceneObjects)
        {
            if(obj == null) continue;
            
            obj.Shader.Bind();
            // obj.Shader.SetUniformMatrix4("model", false, ref model);
            // obj.Shader.SetUniform3("lightDir", ref lightDir);
            // obj.Shader.SetUniform1("diffuseTexture", 0);
            
            var camView = m_camera.View;
            var camProj = m_camera.Projection;
            var modelMat = obj.ModelMatrix;
            var lightSpaceMatrix = m_directionalLight.LightView * m_directionalLight.LightProjection;
            var textureLoc = obj.TextureData.Texture;
            var lightDir = m_directionalLight.LightDirection;
            var lightColor = Vector3.One;
            var ambientColor = Vector3.Zero;

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

            // GL.ActiveTexture(TextureUnit.Texture1);
            // GL.BindTexture(TextureTarget.Texture2D, (int)shadowMapPtr);
            // loc = GL.GetUniformLocation(obj.Shader.ProgramId, "shadowMap");
            // if (loc != -1) GL.Uniform1(loc, 1);

            GL.BindVertexArray(obj.Mesh.Vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, obj.Mesh.VertexCount);

            GL.BindVertexArray(0);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            
            obj.Shader.UnBind();
        }
        m_frameBufferService.UnbindFramebuffer();
        m_frameBufferService.RenderFramebuffer();
        // post process pass
        
        // render frame buffer to camera
    }
}