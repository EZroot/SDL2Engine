
using OpenTK.Mathematics;

namespace SDL2Engine.Core.Addressables.Interfaces;

public interface ISprite
{
    nint TextureId { get; }
    int Width { get; }
    int Height { get; }
    void Update(float deltaTime);
    void Render(IntPtr renderer, Vector3 position, float rotation, Vector2 scale);
}