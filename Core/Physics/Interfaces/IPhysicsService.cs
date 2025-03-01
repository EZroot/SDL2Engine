using System.Numerics;
using BepuPhysics;
using Box2DSharp.Dynamics;

namespace SDL2Engine.Core.Physics.Interfaces;

public interface IPhysicsService
{
    public CollisionDetector CollisionDetector { get; }

    void Initialize(float gravity = -9.81f);
    void RegisterGameObject(GameObject gameObject, float width, float height, BodyType type);
    void UpdatePhysics(float fixedTime);
    // void InterpolateObjects(float deltaTime);
    BodyHandle CreatePhysicsBody(Vector3 position, Vector3 size, float mass);
    void CreateStaticPhysicsBody(Vector3 position, Vector3 size);
    Body CreatePhysicsBody(Vector2 position, float width, float height, BodyType type);
    BodyHandle CreateSpherePhysicsBody(Vector3 position, float radius, float mass);
    BodyReference GetBodyReference(BodyHandle bodyHandle);
    void ApplyLinearImpulse(BodyHandle handle, in Vector3 impulse);
    void CreateWindowBoundaries(float screenWidth, float screenHeight);
    void SetContactListener(IContactListener contactListener);
}