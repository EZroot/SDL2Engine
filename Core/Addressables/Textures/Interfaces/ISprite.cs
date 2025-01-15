using System.Numerics;

namespace SDL2Engine.Core.Addressables.Interfaces;

public interface ISprite
{
    int Width { get; }
    int Height { get; }
    void Update(float deltaTime);
    void Render(IntPtr renderer, Vector2 position, float rotation, Vector2 scale);
}