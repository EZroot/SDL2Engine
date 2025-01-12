using System.Numerics;
using Box2DSharp.Dynamics;
using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;

public class GameObject
{
    // PPM: Pixels Per Meter. Assuming this is globally accessible or passed in.
    private const float PPM = 100f;

    // Other properties...
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
    /// Example Render method. Uses your asset manager to draw.
    /// </summary>
    public virtual void Render(nint renderer, IServiceAssetManager assetManager, IServiceCameraService cameraService = null)
    {
        if (TextureId == 0) return;

        // Calculate scaled width/height (apply scaling)
        int scaledWidth = (int)(OriginalWidth * Scale.X);
        int scaledHeight = (int)(OriginalHeight * Scale.Y);

        // Create an SDL rect for the destination
        var destRect = new SDL.SDL_Rect
        {
            // Convert position to pixels (scaled from meters using PPM)
            x = (int)(Position.X),
            y = (int)(Position.Y),
            w = scaledWidth,
            h = scaledHeight
        };

        // If you have a camera system, apply camera transformations
        if (cameraService != null)
        {
            var camera = cameraService.GetActiveCamera();
            assetManager.DrawTexture(renderer, TextureId, ref destRect, camera);
        }
        else
        {
            assetManager.DrawTexture(renderer, TextureId, ref destRect);
        }
    }
}
