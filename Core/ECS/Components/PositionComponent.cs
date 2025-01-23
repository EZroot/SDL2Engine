

using OpenTK.Mathematics;

namespace SDL2Engine.Core.ECS.Components;

public struct PositionComponent
{
    public Vector2 Position;

    public PositionComponent(Vector2 position)
    {
        Position = position;
    }
}