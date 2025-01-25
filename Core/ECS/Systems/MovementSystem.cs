using System.Diagnostics;
using System.Numerics;
using SDL2Engine.Core.ECS.Components;
using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Engine.Core.ECS.Systems
{
    public class MovementSystem : ISystem
    {
        private readonly ComponentManager componentManager;

        public MovementSystem(ComponentManager componentManager)
        {
            this.componentManager = componentManager;
        }

        public void Update(float deltaTime)
        {
            var positions = componentManager.GetComponentDictionary<PositionComponent>();
            var velocities = componentManager.GetComponentDictionary<VelocityComponent>();

            foreach (var entityId in velocities.Keys)
            {
                if (positions.TryGetValue(entityId, out var position) &&
                    velocities.TryGetValue(entityId, out var velocity))
                {
                    position.Position += velocity.Velocity * deltaTime;

                    // This will overwrite a component if it exists
                    componentManager.AddComponent(new Entity(entityId), position);
                }
            }
        }

        public void Render(IRenderService renderService, ICameraService cameraService)
        {
            // does not handle rendering.
        }
    }
}