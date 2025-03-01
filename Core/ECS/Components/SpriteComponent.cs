using OpenTK.Mathematics;
using SDL2Engine.Core.Addressables.Interfaces;

namespace SDL2Engine.Core.ECS.Components;

public struct SpriteComponent : IComponent
{
    public ISprite Sprite;
    public Vector2 Scale;

    public SpriteComponent(ISprite sprite, Vector2 scale)
    {
        Sprite = sprite;
        Scale = scale;
    }
}