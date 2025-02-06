using OpenTK.Mathematics;

namespace SDL2Engine.Core.Lighting.Interfaces;

public interface ILight
{
    public LightType LightType { get; }
    public Matrix4 LightView { get; }
    public Matrix4 LightProjection { get; }
    public Vector3 LightPosition { get; }
    public Vector3 LightDirection { get; }
    Matrix4 Update(Vector3 pos, Quaternion rotation, float lightDistance);
    Matrix4 Update(Vector3 pos, float lightRotationX, float lightRotationY, float lightRotationZ, float lightDistance);
}