using System.Numerics;
using System.Collections.Generic;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Dynamics;
using SDL2Engine.Core;
using SDL2Engine.Core.Physics.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Physics
{
    public class PhysicsService : IServicePhysicsService
    {
        private World m_world;

        // Keep track of all GameObjects we manage
        private readonly List<GameObject> m_registeredObjects = new List<GameObject>();
        
        public void Initialize(float gravity = -9.81f)
        {
            Vector2 gravityVec = new Vector2(0f, gravity); 
            m_world = new World(gravityVec);
            Debug.Log($"<color=green>PHYSICS ENGINE INITIALIZED gravity: {gravity}</color>");
        }

        /// <summary>
        /// Creates a physics body for the GameObject based on position/width/height,
        /// and registers it so we can auto-sync position/rotation after simulation.
        /// </summary>
        /// <param name="gameObject">The game object to attach a physics body.</param>
        /// <param name="width">Width in physics units (e.g., meters if using a physics scale).</param>
        /// <param name="height">Height in physics units.</param>
        /// <param name="type">The Box2D body type (Static, Dynamic, Kinematic).</param>
        public void RegisterGameObject(GameObject gameObject, float width, float height, BodyType type)
        {
            BodyDef bodyDef = new BodyDef
            {
                BodyType = type,
                Position = gameObject.Position  
            };

            Body body = m_world.CreateBody(bodyDef);
            
            //This only creates box shapes right now
            PolygonShape shape = new PolygonShape();
            shape.SetAsBox(width / 2f, height / 2f);

            FixtureDef fixtureDef = new FixtureDef
            {
                Shape = shape,
                Density = 1.0f,
                Friction = 0.3f
            };

            body.CreateFixture(fixtureDef);
            gameObject.PhysicsBody = body;
            m_registeredObjects.Add(gameObject);
        }

        /// <summary>
        /// Step (simulate) the physics world, then sync positions/rotations back to the registered GameObjects.
        /// </summary>
        public void UpdatePhysics(float deltaTime)
        {
            int velocityIterations = 8;
            int positionIterations = 3;

            m_world.Step(deltaTime, velocityIterations, positionIterations);
            SyncGameObjects();
        }

        /// <summary>
        /// Sync each registered GameObject’s Position/Rotation from its physics body.
        /// </summary>
        private void SyncGameObjects()
        {
            foreach (var gameObject in m_registeredObjects)
            {
                Body body = gameObject.PhysicsBody;
                if (body != null)
                {
                    gameObject.Position = body.GetPosition();
                    gameObject.Rotation = body.GetAngle();
                }
            }
        }

        public Body CreatePhysicsBody(Vector2 position, float width, float height, BodyType type)
        {
            BodyDef bodyDef = new BodyDef
            {
                BodyType = type,
                Position = position
            };

            Body body = m_world.CreateBody(bodyDef);

            PolygonShape shape = new PolygonShape();
            shape.SetAsBox(width / 2f, height / 2f);

            FixtureDef fixtureDef = new FixtureDef
            {
                Shape = shape,
                Density = 1.0f,
                Friction = 0.3f
            };

            body.CreateFixture(fixtureDef);

            return body;
        }
        
        public void SetContactListener(IContactListener contactListener)
        {
            m_world.SetContactListener(contactListener);
        }
    }
}
