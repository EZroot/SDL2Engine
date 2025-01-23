using System;
using System.Collections.Generic;

namespace SDL2Engine.Core.ECS.Components
{
    public static class ComponentType
    {
        private static readonly Dictionary<Type, int> TypeToId = new Dictionary<Type, int>();
        private static int nextId = 0;

        public static int GetId<T>() where T : struct, IComponent
        {
            var type = typeof(T);
            if (TypeToId.TryGetValue(type, out int id))
            {
                return id;
            }

            id = nextId++;
            TypeToId[type] = id;
            return id;
        }
    }
}