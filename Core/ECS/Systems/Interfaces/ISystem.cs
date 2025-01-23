namespace SDL2Engine.Core.ECS.Systems
{
    public interface ISystem
    {
        void Update(float deltaTime);
        void Render(nint renderer);
    }
}