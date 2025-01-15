using System.Numerics;
using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;

namespace SDL2Engine.Core.Addressables.Data;

public class StaticSprite : ISprite
{
    private IntPtr texture;
    public StaticSprite(IntPtr texture)
    {
        this.texture = texture;
    }

    public void Update(float deltaTime)
    {
    }

    public void Render(IntPtr renderer, Vector2 position, float rotation, Vector2 scale)
    {
    }
}