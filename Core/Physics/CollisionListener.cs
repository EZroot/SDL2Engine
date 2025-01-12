using Box2DSharp.Collision.Collider;
using Box2DSharp.Dynamics;
using Box2DSharp.Dynamics.Contacts;

namespace SDL2Engine.Core.Physics;

/// <summary>
/// Example of a collision detector
/// </summary>
public class CollisionListener : IContactListener
{
    public void BeginContact(Contact contact)
    {
        throw new NotImplementedException();
    }

    public void EndContact(Contact contact)
    {
        throw new NotImplementedException();
    }

    public void PreSolve(Contact contact, in Manifold oldManifold)
    {
        throw new NotImplementedException();
    }

    public void PostSolve(Contact contact, in ContactImpulse impulse)
    {
        throw new NotImplementedException();
    }
}