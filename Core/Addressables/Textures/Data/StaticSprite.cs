using System.Numerics;
using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Addressables.Data;

public class StaticSprite : ISprite
{
    private IntPtr texture;
    private int textureWidth;
    private int textureHeight;

    public int Width => textureWidth;
    public int Height => textureHeight;

    public StaticSprite(IntPtr texture, int textureWidth, int textureHeight)
    {
        this.texture = texture;
        this.textureWidth = textureWidth;
        this.textureHeight = textureHeight;
    }
    
    public void Update(float deltaTime)
    {
    }

    public void Render(IntPtr renderer, Vector2 position, float rotation, Vector2 scale)
    {
        SDL.SDL_Rect sourceRect = new SDL.SDL_Rect
        {
            x = 0,
            y = 0,
            w = textureWidth,  
            h = textureHeight
        };

        SDL.SDL_Rect destRect = new SDL.SDL_Rect
        {
            x = (int)position.X,
            y = (int)position.Y,
            w = (int)(textureWidth * scale.X),
            h = (int)(textureHeight * scale.Y)
        };

        SDL.SDL_Point center = new SDL.SDL_Point
        {
            x = destRect.w / 2,
            y = destRect.h / 2
        };

        SDL.SDL_RenderCopyEx(renderer, texture, ref sourceRect, ref destRect, rotation, ref center, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
    }

}