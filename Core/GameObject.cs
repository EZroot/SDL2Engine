using Box2DSharp.Dynamics;
using OpenTK.Mathematics;
using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Partitions;
using SDL2Engine.Core.Partitions.Interfaces;
using SDL2Engine.Core.Cameras.Interfaces;
using SDL2Engine.Core.Utils;

public class GameObject
{
    public GameObject(ISprite sprite, Vector3 position, Vector2 scale, IPartitioner partitioner = null)
    {
        Sprite = sprite;
        Position = position;
        Scale = scale;
        if (partitioner != null)
        {
            m_partitioner = partitioner;
            m_partitioner.Add(this);
        }
    }

    // PPM: Pixels Per Meter.
    private const float PPM = 100f;

    private IPartitioner m_partitioner;
    private Vector3 m_position;
    private Vector3 m_lastPosition; 
    private (int, int)? m_currentCell;

    public Vector3 Position
    {
        get => m_position;
        set => m_position = value; 
    }
    public Vector3 Velocity { get; set; } 
    public float Rotation { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;

    public (int, int)? CurrentCell
    {
        get => m_currentCell;
        set => m_currentCell = value;
    }

    public Body PhysicsBody { get; set; } 

    public ISprite Sprite { get; set; }

    /// <summary>
    /// Sync position/rotation from PhysicsBody if present, update partitioner, and update sprite
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        // Sync with physics
        if (PhysicsBody != null)
        {
            var pos = PhysicsBody.GetPosition();
            Position = new Vector3(pos.X,pos.Y,0) * PPM;
            Rotation = (float)(PhysicsBody.GetAngle() * (180 / Math.PI));
        }
        else
        {
            Position += Velocity;
        }
        
        if (m_partitioner != null && Position != m_lastPosition)
        {
            m_partitioner.Update(this);
            m_lastPosition = Position; 
        }

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
            Vector3 cameraAdjustedPosition = Position - cameraOffset;
            Sprite.Render(renderer, cameraAdjustedPosition, Rotation, Scale);
        }
        else
        {
            Sprite.Render(renderer, Position, Rotation, Scale);
        }
    }
}
