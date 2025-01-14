using System.Numerics;
using Box2DSharp.Dynamics;
using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;

public class GameObject
{
    // PPM: Pixels Per Meter. Assuming this is globally accessible or passed in.
    private const float PPM = 100f;

    public Vector2 Position { get; set; }
    public float Rotation { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;

    public Vector2 PreviousPosition { get; set; }
    public float PreviousRotation { get; set; }
    public Vector2 CurrentPosition { get; set; }
    public float CurrentRotation { get; set; }

    public Body PhysicsBody { get; set; }  // Box2DSharp body

    // Visual / Rendering properties
    public int TextureId { get; set; }
    public int OriginalWidth { get; set; }
    public int OriginalHeight { get; set; }

    /// <summary>
    /// Sync position/rotation from PhysicsBody if present
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        // Sync with physics body if present
        if (PhysicsBody != null)
        {
            // Update position in pixels using PPM scaling
            Position = PhysicsBody.GetPosition() * PPM;
            Rotation = PhysicsBody.GetAngle();
        }
    }

    /// <summary>
    /// Render Gameobject
    /// </summary>
    public virtual void Render(nint renderer, IAssetService assetManager, ICameraService cameraService = null)
    {
        if (TextureId == 0)
        {
            Debug.LogError("TextureId is 0. Skipping render.");
            return;
        }

        int scaledWidth = (int)(OriginalWidth * Scale.X);
        int scaledHeight = (int)(OriginalHeight * Scale.Y);

        var destRect = new SDL.SDL_Rect
        {
            x = (int)(Position.X),
            y = (int)(Position.Y),
            w = scaledWidth,
            h = scaledHeight
        };

        SDL.SDL_Point center = new SDL.SDL_Point
        {
            x = scaledWidth / 2,
            y = scaledHeight / 2
        };

        if (cameraService != null)
        {
            var camera = cameraService.GetActiveCamera();
            if (camera != null)
            {
                assetManager.DrawTextureWithRotation(renderer, TextureId, ref destRect, Rotation, ref center, camera);
            }
            else
            {
                Debug.LogError("Active camera is null.");
            }
        }
        else
        {
            assetManager.DrawTextureWithRotation(renderer, TextureId, ref destRect, Rotation, ref center);
        }
    }

}
