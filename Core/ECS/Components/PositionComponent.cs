using System.Numerics;

namespace SDL2Engine.Core.ECS.Components
{
    public struct PositionComponent : IComponent
    {
        private Vector2 _position;
        private Vector2 _oldPosition;
        public Vector2 Position
        {
            get => _position;
            set
            {
                _oldPosition = _position;
                _position = value;
            }
        }

        public Vector2 OldPosition => _oldPosition;

        public PositionComponent(Vector2 position)
        {
            Position = position;
        }
    }
}