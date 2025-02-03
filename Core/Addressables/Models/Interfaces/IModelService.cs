using OpenTK.Mathematics;

namespace SDL2Engine.Core.Addressables.Models.Interfaces;

public interface IModelService
{
    OpenGLHandle Load3DModel(string path, string vertShaderSrc, string fragShaderSrc, float aspect);

    void DrawModelGL(OpenGLHandle glHandle, Matrix4 modelMatrix, CameraGL3D camera, nint texturePointer);
}