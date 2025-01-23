using System.Collections.Generic;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.ECS.Components;
using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Engine.Core.ECS.Systems
{
    public class SystemManager
    {
        private readonly List<ISystem> systems = new List<ISystem>();

        public SystemManager()
        {
        }

        public void RegisterSystem(ISystem system)
        {
            systems.Add(system);
        }

        public void Update(float deltaTime)
        {
            foreach (var system in systems)
            {
                system.Update(deltaTime);
            }
        }

        public void Render(IRenderService renderService, ICameraService cameraService)
        {
            foreach (var system in systems)
            {
                system.Render(renderService, cameraService);
            }
        }
    }
}