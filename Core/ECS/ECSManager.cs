using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.ECS.Components;
using SDL2Engine.Core.ECS.Systems;
using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Engine.Core.ECS;
    public class ECSManager
    {
        public ComponentManager<PositionComponent> PositionComponents { get; } = new ComponentManager<PositionComponent>();
        public ComponentManager<SpriteComponent> SpriteComponents { get; } = new ComponentManager<SpriteComponent>();

        private readonly List<Entity> entities = new List<Entity>();
        private readonly MovementSystem movementSystem;
        private readonly RenderingSystem renderingSystem;

        private readonly nint renderer;
        private readonly ICameraService cameraService;
        private readonly IImageService imageService;
        public ECSManager(IRenderService renderService, IImageService imageService, ICameraService cameraService)
        {
            this.renderer = renderService.RenderPtr;
            this.cameraService = cameraService;
            this.imageService = imageService;
            movementSystem = new MovementSystem();
            renderingSystem = new RenderingSystem(imageService, renderService);
        }

        public Entity CreateEntity()
        {
            var entity = new Entity(entities.Count);
            entities.Add(entity);
            return entity;
        }

        public void AddComponent<T>(Entity entity, T component) where T : struct
        {
            if (typeof(T) == typeof(PositionComponent))
            {
                PositionComponents.AddComponent(entity.Id, (PositionComponent)(object)component);
            }
            else if (typeof(T) == typeof(SpriteComponent))
            {
                SpriteComponents.AddComponent(entity.Id, (SpriteComponent)(object)component);
            }
        }

        public void Update(float deltaTime)
        {
            movementSystem.Update(entities, PositionComponents);
        }

        public void Render()
        {
            renderingSystem.Render(entities, PositionComponents, SpriteComponents, renderer, cameraService);
        }

        public IEnumerable<Entity> GetEntitiesWith<T1, T2>() where T1 : struct where T2 : struct
        {
            foreach (var entity in entities)
            {
                if (PositionComponents.TryGetComponent(entity.Id, out _) && SpriteComponents.TryGetComponent(entity.Id, out _))
                {
                    yield return entity;
                }
            }
        }
    }