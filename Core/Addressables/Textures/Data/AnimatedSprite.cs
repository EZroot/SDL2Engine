using System.Numerics;
using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;

namespace SDL2Engine.Core.Addressables.Data;

public class AnimatedSprite : ISprite
{
    private IntPtr texture;   // The texture that holds the sprite sheet
    private int frameWidth;
    private int frameHeight;
    private int totalFrames;
    private int currentFrame;
    private float frameTime;  // Time each frame is displayed
    private float timeSinceLastFrame;

    public AnimatedSprite(IntPtr texture, int frameWidth, int frameHeight, int totalFrames, float frameTime)
    {
        this.texture = texture;
        this.frameWidth = frameWidth;
        this.frameHeight = frameHeight;
        this.totalFrames = totalFrames;
        this.frameTime = frameTime;
        this.currentFrame = 0;
        this.timeSinceLastFrame = 0.0f;
    }

    public void Update(float deltaTime)
    {
        timeSinceLastFrame += deltaTime;
        if (timeSinceLastFrame >= frameTime)
        {
            currentFrame++;
            if (currentFrame >= totalFrames)
                currentFrame = 0;
            timeSinceLastFrame = 0.0f;
        }
    }

    public void Render(IntPtr renderer, Vector2 position, float rotation, Vector2 scale)
    {
        SDL.SDL_Rect sourceRect = new SDL.SDL_Rect
        {
            x = currentFrame * frameWidth,
            y = 0,
            w = frameWidth,
            h = frameHeight
        };

        SDL.SDL_Rect destRect = new SDL.SDL_Rect
        {
            x = (int)position.X,
            y = (int)position.Y,
            w = frameWidth,
            h = frameHeight
        };

        SDL.SDL_RenderCopy(renderer, texture, ref sourceRect, ref destRect);
    }
}
