using SDL2;
using SDL2Engine.Core.Addressables.Data;
using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Engine.Core.Addressables.Interfaces;

public interface IImageService
{
    nint LoadImageRaw(string path);
    TextureData LoadTexture(IntPtr renderer, string path);
    void DrawTexture(IntPtr renderer, int textureId, ref SDL.SDL_Rect dstRect);
    void DrawTexture(IntPtr renderer, int textureId, ref SDL.SDL_Rect dstRect, ICamera camera);
    void DrawTextureWithRotation(nint renderer, int textureId, ref SDL.SDL_Rect destRect, float rotation, ref SDL.SDL_Point center);
    void DrawTextureWithRotation(nint renderer, int textureId, ref SDL.SDL_Rect destRect, float rotation, ref SDL.SDL_Point center, ICamera camera);
    void UnloadTexture(int id);
    void Cleanup();
}