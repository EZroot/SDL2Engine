using OpenTK.Mathematics;
using SDL2;
using SDL2Engine.Core.Addressables.Data;
using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Engine.Core.Addressables.Interfaces;

public interface IImageService
{
    nint LoadImageRaw(string path);
    TextureData LoadTexture(IntPtr renderer, string path);
    void DrawTextureGL(OpenGLHandle glHandler, nint textureId, ICamera camera, Matrix4 modelMatrix);
    void DrawTexturesGLBatched(OpenGLHandle glHandler, int[] textureIds, ICamera camera, Matrix4[] modelMatrices);
    void DrawCubeGL(OpenGLHandle glHandler, Matrix4 modelMatrix, CameraGL3D camera, nint texturePointer);
    void DrawTexture(IntPtr renderer, int textureId, ref SDL.SDL_Rect dstRect, ICamera camera);
    void DrawTextureWithRotation(nint renderer, int textureId, ref SDL.SDL_Rect destRect, float rotation, ref SDL.SDL_Point center, ICamera camera);
    void UnloadTexture(int id);
    void Cleanup();
}