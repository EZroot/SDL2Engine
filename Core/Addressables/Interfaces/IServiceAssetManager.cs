using SDL2;
using static SDL2Engine.Core.Addressables.AssetManager;

namespace SDL2Engine.Core.Addressables.Interfaces
{
    public interface IServiceAssetManager
    {
        IntPtr LoadSound(string path, AudioType audioType = AudioType.Wave);
        void PlaySound(IntPtr soundEffect, int volume = 128, bool isMusic = false);
        void UnloadSound(IntPtr soundEffect);
        TextureData LoadTexture(IntPtr renderer, string path);
        void DrawTexture(IntPtr renderer, int textureId, ref SDL.SDL_Rect dstRect);
        void UnloadTexture(int id);
        void Cleanup();
    }
}
