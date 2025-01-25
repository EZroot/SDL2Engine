using SDL2Engine.Core.ECS.Components;
using SDL2Engine.Core.Partitions;
using SDL2Engine.Core.Rendering;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.ECS.Systems
{
    public class SpatialPartitionerSystem : ISystem
    {
        private readonly SpatialPartitionerECS spatialPartitioner;
        private readonly ComponentManager componentManager;

        public SpatialPartitionerSystem(SpatialPartitionerECS spatialPartitioner, ComponentManager componentManager)
        {
            this.spatialPartitioner = spatialPartitioner;
            this.componentManager = componentManager;
        }

        public void Update(float deltaTime)
        {
            // Retrieve all entities with PositionComponent and optionally CurrentCellComponent
            var entities = componentManager.GetEntitiesWith<PositionComponent>();

            foreach (var entity in entities)
            {
                if (componentManager.TryGetComponent(entity, out PositionComponent positionComponent))
                {
                    if (componentManager.TryGetComponent(entity, out CurrentCellComponent currentCellComp))
                    {
                        spatialPartitioner.UpdateEntity(entity, positionComponent.OldPosition);
                    }
                    else
                    {
                        // Entity is not yet in any cell, add it
                        spatialPartitioner.Add(entity);
                    }
                }
            }
        }

        public void Render(IRenderService renderService, ICameraService cameraService)
        {
            var camera = (CameraGL)cameraService.GetActiveCamera();
            spatialPartitioner.RenderDebug(renderService, camera.Projection, cameraService);
            // spatialPartitioner.PrintDebugInfo();
        }
    }
}