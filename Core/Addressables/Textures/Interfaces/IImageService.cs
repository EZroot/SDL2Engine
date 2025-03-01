using OpenTK.Mathematics;
using SDL2;
using SDL2Engine.Core.Addressables.Data;
using SDL2Engine.Core.Cameras.Interfaces;

namespace SDL2Engine.Core.Addressables.Interfaces;

public interface IImageService
{
    nint LoadImageRaw(string path);
    TextureData LoadTexture(string path);
    void DrawTextureGL(OpenGLHandle glHandler, nint textureId, ICamera camera, Matrix4 modelMatrix);
    void DrawTexturesGLBatched(OpenGLHandle glHandler, int[] textureIds, ICamera camera, Matrix4[] modelMatrices);
    void DrawTexture(int textureId, ref SDL.SDL_Rect dstRect, ICamera camera);
    void DrawTextureWithRotation(int textureId, ref SDL.SDL_Rect destRect, float rotation, ref SDL.SDL_Point center, ICamera camera);
    void UnloadTexture(int id);
    void Cleanup();
}