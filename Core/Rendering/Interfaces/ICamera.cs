
using OpenTK.Mathematics;

namespace SDL2Engine.Core.Rendering.Interfaces;

public interface ICamera
{
    public Vector3 Position { get; }
    public float Zoom { get; }
    public string Name { get; }
    
    void Move(Vector3 delta);
    void SetPosition(Vector3 newPosition);
    void SetZoom(float newZoom);
    Vector3 GetOffset();
    void SetName(string name);
}