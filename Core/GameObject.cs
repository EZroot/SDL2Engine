using System.Numerics;
using Box2DSharp.Dynamics;
using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;

public class GameObject
{
    public GameObject(ISprite sprite, Vector2 position, Vector2 scale)
    {
        Sprite = sprite;
        Position = position;
        Scale = scale;
    }

    // PPM: Pixels Per Meter. Assuming this is globally accessible or passed in.
    private const float PPM = 100f;

    public Vector2 Position { get; set; }
    public float Rotation { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;

    public Body PhysicsBody { get; set; }  // Box2DSharp body

    // Sprite property for rendering
    public ISprite Sprite { get; set; }

    /// <summary>
    /// Sync position/rotation from PhysicsBody if present and update sprite
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        // Sync with physics body if present
        if (PhysicsBody != null)
        {
            Position = PhysicsBody.GetPosition() * PPM;
            Rotation = (float)(PhysicsBody.GetAngle() * (180 / Math.PI));
        }
        
        // Update sprite animation or state
        Sprite?.Update(deltaTime);
    }

    /// <summary>
    /// Render GameObject using its sprite
    /// </summary>
    public virtual void Render(nint renderer, ICameraService cameraService = null)
    {
        if (Sprite == null)
        {
            Debug.LogError("Sprite is not set. Skipping render.");
            return;
        }

        // If there's an active camera, adjust position by camera offset before rendering
        if (cameraService != null)
        {
            var cameraOffset = cameraService.GetActiveCamera().GetOffset();
            Vector2 cameraAdjustedPosition = Position - cameraOffset;
            Sprite.Render(renderer, cameraAdjustedPosition, Rotation, Scale);
        }
        else
        {
            Sprite.Render(renderer, Position, Rotation, Scale);
        }
    }
}