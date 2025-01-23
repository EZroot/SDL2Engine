using SDL2Engine.Core.ECS.Components;

namespace SDL2Engine.Core.ECS.Systems;


public class MovementSystem
{
    public void Update(IEnumerable<Entity> entities, ComponentManager<PositionComponent> positionComponents)
    {
        var positions = positionComponents.GetAllComponents(entities);

        foreach (var (entityId, position) in positions)
        {
            // Add velocity and other logic as needed.
        }
    }
}