using OpenTK.Mathematics;
using SDL2Engine.Core.Cameras;

namespace SDL2Engine.Core.Addressables.Models.Interfaces;

public interface IModelService
{
    public int FboColorTexture { get; }
    public void BindFramebuffer(int screenWidth, int screenHeight);
    public void UnbindFramebuffer();
    public void RenderFramebuffer();
    
    OpenGLHandle Load3DModel(string path, string vertShaderPath, string fragShaderPath, float aspect);
    OpenGLHandle Create3DArrow(string vertShaderPath, string fragShaderPath);
    OpenGLHandle CreateCube(string vertShaderPath, string fragShaderPath, float aspect);
    OpenGLHandle CreateQuad(string vertShaderPath, string fragShaderPath, float aspect);
    OpenGLHandle CreateSphere(string vertShaderPath, string fragShaderPath, float aspect, int sectorCount = 36,
        int stackCount = 18);

    OpenGLHandle CreateFullscreenQuad(string vertShaderPath, string fragShaderPath);
    void DrawArrow(OpenGLHandle arrowHandle, CameraGL3D cam, Vector3 position, Vector3 dirNormalized);
    void DrawModelGL(
        OpenGLHandle glHandle,
        Matrix4 modelMatrix,
        CameraGL3D camera,
        nint diffuseTexturePointer,
        Matrix4 lightSpaceMatrix,
        nint shadowMapPointer,
        Vector3 lightDir,
        Vector3 lightColor,
        Vector3 ambientColor);
}