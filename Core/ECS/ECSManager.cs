using System;
using System.Collections.Generic;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.ECS.Components;
using SDL2Engine.Core.ECS.Systems;
using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Engine.Core.ECS
{
    public class ECSManager
    {
        private readonly EntityManager entityManager = new EntityManager();
        private readonly ComponentManager componentManager = new ComponentManager();
        private readonly SystemManager systemManager;

        private readonly nint renderer;
        private readonly ICameraService cameraService;
        private readonly IImageService imageService;
        
        public ComponentManager ComponentManager => componentManager;

        public ECSManager(IRenderService renderService, IImageService imageService, ICameraService cameraService)
        {
            this.renderer = renderService.RenderPtr;
            this.cameraService = cameraService;
            this.imageService = imageService;
            systemManager = new SystemManager();
        }

        public Entity CreateEntity()
        {
            return entityManager.CreateEntity();
        }

        public void DestroyEntity(Entity entity)
        {
            entityManager.DestroyEntity(entity);
            componentManager.RemoveAllComponents(entity);
        }

        public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
        {
            componentManager.AddComponent(entity, component);
        }

        public bool TryGetComponent<T>(Entity entity, out T component) where T : struct, IComponent
        {
            return componentManager.TryGetComponent(entity, out component);
        }

        public void RegisterSystem(ISystem system)
        {
            systemManager.RegisterSystem(system);
        }

        public void Update(float deltaTime)
        {
            systemManager.Update(deltaTime);
        }

        public void Render()
        {
            systemManager.Render(renderer);
        }

        public IEnumerable<Entity> GetEntitiesWith<T1, T2>() 
            where T1 : struct, IComponent 
            where T2 : struct, IComponent
        {
            var dict1 = componentManager.GetComponentDictionary<T1>();
            var dict2 = componentManager.GetComponentDictionary<T2>();

            foreach (var entityId in dict1.Keys)
            {
                if (dict2.ContainsKey(entityId))
                {
                    yield return new Entity(entityId);
                }
            }
        }

        /// <summary>
        /// Retrieves all entities that have all specified component types.
        /// </summary>
        public IEnumerable<Entity> GetEntitiesWith<T1, T2, T3>()
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            return componentManager.GetEntitiesWith<T1, T2, T3>();
        }
    }
}
