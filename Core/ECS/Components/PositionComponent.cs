using System.Numerics;

namespace SDL2Engine.Core.ECS.Components
{
    public struct PositionComponent : IComponent
    {
        public Vector2 Position;

        public PositionComponent(Vector2 position)
        {
            Position = position;
        }
    }
}