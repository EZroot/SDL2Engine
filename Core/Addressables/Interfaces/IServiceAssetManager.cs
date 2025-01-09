using SDL2;
using static SDL2Engine.Core.Addressables.AssetManager;

namespace SDL2Engine.Core.Addressables.Interfaces
{
    public interface IServiceAssetManager
    {
        public TextureData LoadTexture(IntPtr renderer, string path);
        public void DrawTexture(IntPtr renderer, int textureId, ref SDL.SDL_Rect dstRect);
        public void UnloadTexture(int id);
        public void Cleanup();
    }
}
