using BepuPhysics;
using OpenTK.Mathematics;
using SDL2Engine.Core.Addressables.Data;
using SDL2Engine.Core.Geometry;

namespace SDL2Engine.Core.World;

public class GameObject3D
{
    public Mesh Mesh { get; private set;}
    public TextureData TextureData { get; private set;}
    public Shader Shader { get; private set;}
    
    public BodyHandle? Rigidbody { get; private set; }
    public Vector3 Position { get; private set; }
    public Quaternion Rotation { get; private set;}
    public Vector3 Scale { get; private set;}
    public Matrix4 ModelMatrix { get; private set;}
    public bool CastShadows { get; private set; }
    public Vector3 Color { get; private set; }
    public Vector3 AmbientColor { get; private set; }
    
    public GameObject3D(Mesh mesh, TextureData diffuseTexture, Shader shader, BodyHandle? rigidbody = null)
    {
        Mesh = mesh;
        TextureData = diffuseTexture;
        Shader = shader;
        Position = Vector3.Zero;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
        Color = new Vector3(1, 1, 1);
        AmbientColor = new Vector3(0f, 0f, 0.1f);
        Rigidbody = rigidbody;
        UpdateModelMatrix();
    }
    
    public GameObject3D(Mesh mesh, TextureData diffuseTexture, Shader shader, Vector3 color, Vector3 ambientColor, BodyHandle? rigidbody = null)
    {
        Mesh = mesh;
        TextureData = diffuseTexture;
        Shader = shader;
        Position = Vector3.Zero;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
        Color = color;
        AmbientColor = ambientColor;
        Rigidbody = rigidbody;
        UpdateModelMatrix();
    }
    
    public GameObject3D(Mesh mesh, TextureData diffuseTexture, Shader shader, Vector3 pos, Quaternion rot, Vector3 scale, BodyHandle? rigidbody = null)
    {
        Mesh = mesh;
        TextureData = diffuseTexture;
        Shader = shader;
        Position = pos;
        Rotation = rot;
        Scale = scale;
        Rigidbody = rigidbody;
        UpdateModelMatrix();
    }

    public void SetPosition(Vector3 position)
    {
        Position = position;
        UpdateModelMatrix();
    }

    public void SetRotation(Quaternion rotation)
    {
        Rotation = rotation;
        UpdateModelMatrix();
    }

    public void SetScale(Vector3 scale)
    {
        Scale = scale;
        UpdateModelMatrix();
    }

    public void SetCastShadows(bool shouldCastShadows)
    {
        CastShadows = shouldCastShadows;
    }

    private void UpdateModelMatrix()
    {
        ModelMatrix = Matrix4.CreateFromQuaternion(Rotation) * MathHelper.GetMatrixTranslation(Position, Scale);
    }
}