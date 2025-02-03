using OpenTK.Mathematics;
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

    public nint TextureId => texture;
    public int Width => frameWidth;
    public int Height => frameHeight;
    
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

    public void Render(IntPtr renderer, Vector3 position, float rotation, Vector2 scale)
    {
        SDL.SDL_Rect sourceRect = new SDL.SDL_Rect
        {
            x = currentFrame * frameWidth,
            y = 0,
            w = frameWidth,
            h = frameHeight
        };

        int scaledWidth = (int)(frameWidth * scale.X);
        int scaledHeight = (int)(frameHeight * scale.Y);

        SDL.SDL_Rect destRect = new SDL.SDL_Rect
        {
            x = (int)(position.X - scaledWidth / 2f),
            y = (int)(position.Y - scaledHeight / 2f),
            w = scaledWidth,
            h = scaledHeight
        };

        SDL.SDL_Point center = new SDL.SDL_Point
        {
            x = scaledWidth / 2,
            y = scaledHeight / 2
        };

        SDL.SDL_RenderCopyEx(renderer, texture, ref sourceRect, ref destRect, rotation, ref center, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
    }
}
