using System;
using System.Collections.Generic;

namespace SDL2Engine.Core.ECS.Components
{
    public class ComponentManager
    {
        private readonly Dictionary<int, IComponent[]> components = new Dictionary<int, IComponent[]>();
        private readonly Dictionary<int, Dictionary<int, IComponent>> componentStores = new Dictionary<int, Dictionary<int, IComponent>>();

        public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
        {
            int typeId = ComponentType.GetId<T>();

            if (!componentStores.TryGetValue(typeId, out var store))
            {
                store = new Dictionary<int, IComponent>();
                componentStores[typeId] = store;
            }

            store[entity.Id] = component;
        }

        public bool TryGetComponent<T>(Entity entity, out T component) where T : struct, IComponent
        {
            int typeId = ComponentType.GetId<T>();
            component = default;

            if (componentStores.TryGetValue(typeId, out var store) && store.TryGetValue(entity.Id, out var obj))
            {
                component = (T)obj;
                return true;
            }

            return false;
        }

        public void RemoveComponent<T>(Entity entity) where T : struct, IComponent
        {
            int typeId = ComponentType.GetId<T>();

            if (componentStores.TryGetValue(typeId, out var store))
            {
                store.Remove(entity.Id);
            }
        }

        public IEnumerable<T> GetComponents<T>() where T : struct, IComponent
        {
            int typeId = ComponentType.GetId<T>();

            if (componentStores.TryGetValue(typeId, out var store))
            {
                foreach (var component in store.Values)
                {
                    yield return (T)component;
                }
            }
        }

        public Dictionary<int, T> GetComponentDictionary<T>() where T : struct, IComponent
        {
            int typeId = ComponentType.GetId<T>();
            var result = new Dictionary<int, T>();

            if (componentStores.TryGetValue(typeId, out var store))
            {
                foreach (var kvp in store)
                {
                    result[kvp.Key] = (T)kvp.Value;
                }
            }

            return result;
        }

        public void RemoveAllComponents(Entity entity)
        {
            foreach (var store in componentStores.Values)
            {
                store.Remove(entity.Id);
            }
        }
    }
}
