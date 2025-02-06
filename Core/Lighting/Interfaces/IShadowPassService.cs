using OpenTK.Mathematics;

namespace SDL2Engine.Core.Lighting.Interfaces;

public interface IShadowPassService
{
    int DepthTexturePtr { get; }
    void Initialize(int shadowWidth = 2048, int shadowHeight = 2048);
    void RegisterMesh(OpenGLHandle asset, Matrix4 model);
    void UpdateMeshModel(OpenGLHandle asset, Matrix4 newModel);
    void RenderShadowPass(Matrix4 lightView, Matrix4 lightProjection);
    void RenderDebugQuad();
}