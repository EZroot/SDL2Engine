using System.Numerics;
using Box2DSharp.Dynamics;

namespace SDL2Engine.Core.Physics.Interfaces;

public interface IServicePhysicsService
{
    void Initialize(float gravity = -9.81f);
    void RegisterGameObject(GameObject gameObject, float width, float height, BodyType type);
    void UpdatePhysics(float deltaTime);
    Body CreatePhysicsBody(Vector2 position, float width, float height, BodyType type);
    void SetContactListener(IContactListener contactListener);
}