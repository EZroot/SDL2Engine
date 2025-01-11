using System.Numerics;

namespace SDL2Engine.Core.Rendering.Interfaces;

public interface ICamera
{
    public Vector2 Position { get; }
    public float Zoom { get; }
    public string Name { get; }
    
    void Move(Vector2 delta);
    void SetPosition(Vector2 newPosition);
    void SetZoom(float newZoom);
    Vector2 GetOffset();
    void SetName(string name);
}