using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Engine.Core.ECS.Systems
{
    public interface ISystem
    {
        void Update(float deltaTime);
        void Render(IRenderService renderService, ICameraService cameraService);
    }
}