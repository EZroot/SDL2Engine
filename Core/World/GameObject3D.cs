using OpenTK.Mathematics;
using SDL2Engine.Core.Addressables.Data;
using SDL2Engine.Core.Geometry;

namespace SDL2Engine.Core.World;

public class GameObject3D
{
    public Mesh Mesh { get; }
    public TextureData TextureData { get; }
    public Shader Shader { get; }

    public Vector3 Position { get; private set; }
    public Quaternion Rotation { get; private set;}
    public Vector3 Scale { get; private set;}
    public Matrix4 ModelMatrix { get; private set;}

    public GameObject3D(Mesh mesh, TextureData diffuseTexture, Shader shader)
    {
        Mesh = mesh;
        TextureData = diffuseTexture;
        Shader = shader;
        Position = Vector3.Zero;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
        UpdateModelMatrix();
    }
    
    public GameObject3D(Mesh mesh, TextureData diffuseTexture, Shader shader, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        Mesh = mesh;
        TextureData = diffuseTexture;
        Shader = shader;
        Position = pos;
        Rotation = rot;
        Scale = scale;
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

    private void UpdateModelMatrix()
    {
        // Might have to update create from quaternion, if objects spin on a offset pivot thats prob why!
        ModelMatrix = MathHelper.GetMatrixTranslation(Position, Scale) * Matrix4.CreateFromQuaternion(Rotation);
    }
}