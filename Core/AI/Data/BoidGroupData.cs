using System.Numerics;
using SDL2Engine.Core.Addressables.Interfaces;

namespace SDL2Engine.Core.AI.Data;

public struct BoidGroupData
{
    public ISprite Sprite;
    public Vector2 Position;
    public Vector2 Scale;

    public BoidGroupData(ISprite sprite, Vector2 position, Vector2 scale)
    {
        Sprite = sprite;
        Position = position;
        Scale = scale;
    }
}