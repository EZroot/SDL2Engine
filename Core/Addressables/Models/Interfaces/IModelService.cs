using OpenTK.Mathematics;

namespace SDL2Engine.Core.Addressables.Models.Interfaces;

public interface IModelService
{
    OpenGLHandle Load3DModel(string path, string vertShaderPath, string fragShaderPath, float aspect);
    OpenGLHandle Load3DArrow(string vertShaderPath, string fragShaderPath);
    OpenGLHandle LoadCube(string vertShaderPath, string fragShaderPath, float aspect);
    OpenGLHandle LoadQuad(string vertShaderPath, string fragShaderPath, float aspect);
    OpenGLHandle LoadSphere(string vertShaderPath, string fragShaderPath, float aspect, int sectorCount = 36,
        int stackCount = 18);

    void DrawLightArrow(OpenGLHandle arrowHandle, CameraGL3D cam, Vector3 position, Vector3 dirNormalized);
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