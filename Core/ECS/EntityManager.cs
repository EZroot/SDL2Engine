using System.Collections.Generic;

namespace SDL2Engine.Core.ECS
{
    public class EntityManager
    {
        private int nextEntityId = 0;
        private readonly Stack<int> recycledIds = new Stack<int>();

        public Entity CreateEntity()
        {
            int id = recycledIds.Count > 0 ? recycledIds.Pop() : nextEntityId++;
            return new Entity(id);
        }

        public void DestroyEntity(Entity entity)
        {
            recycledIds.Push(entity.Id);
            // Optionally, notify component managers to remove components associated with this entity.
        }
    }
}