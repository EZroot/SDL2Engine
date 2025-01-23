namespace SDL2Engine.Core.ECS;

public class ComponentManager<T> where T : struct
{
    private readonly Dictionary<int, T> components = new Dictionary<int, T>();

    public void AddComponent(int entityId, T component)
    {
        components[entityId] = component;
    }

    public bool TryGetComponent(int entityId, out T component)
    {
        return components.TryGetValue(entityId, out component);
    }

    public void RemoveComponent(int entityId)
    {
        components.Remove(entityId);
    }

    public IEnumerable<(int EntityId, T Component)> GetAllComponents(IEnumerable<Entity> entities)
    {
        foreach (var entity in entities)
        {
            if (components.TryGetValue(entity.Id, out var component))
            {
                yield return (entity.Id, component);
            }
        }
    }
}