using System.Diagnostics;
using System.Numerics;
using Box2DSharp.Collision;
using Box2DSharp.Common;
using Box2DSharp.Dynamics;

namespace SDL2Engine.Core.Physics;

public class PointQueryCallback : IQueryCallback
{
    public Fixture FixtureUnderPoint { get; private set; }
    private readonly Vector2 _point;

    public PointQueryCallback(Vector2 point)
    {
        _point = point;
        FixtureUnderPoint = null;
    }

    public bool QueryCallback(Fixture fixture)
    {
        if (fixture.TestPoint(_point))
        {
            FixtureUnderPoint = fixture;
        }
        return true;
    }
}

public class CollisionDetector
{
    private Box2DSharp.Dynamics.World m_world;
    private float m_scale;

    public CollisionDetector(Box2DSharp.Dynamics.World world, float scale = 100f)
    {
        this.m_world = world;
        this.m_scale = scale;
    }
    
    public Body GetBodyUnderPoint(int x, int y)
    {
        float worldX = x / m_scale;
        float worldY = y / m_scale;

        Vector2 queryPoint = new Vector2(worldX, worldY);
        PointQueryCallback callback = new PointQueryCallback(queryPoint);

        float aabbSize = 0.01f;
        AABB aabb = new AABB();
        aabb.LowerBound.Set(queryPoint.X - aabbSize, queryPoint.Y - aabbSize);
        aabb.UpperBound.Set(queryPoint.X + aabbSize, queryPoint.Y + aabbSize);

        m_world.QueryAABB(callback, aabb);

        if (callback.FixtureUnderPoint != null)
        {
            if (callback.FixtureUnderPoint.Body != null)
            {
                return callback.FixtureUnderPoint.Body;
            }
            else
            {
                Utils.Debug.LogError($"Fixture under mouse: Body == null!");
            }
        }

        return null;
    }
}