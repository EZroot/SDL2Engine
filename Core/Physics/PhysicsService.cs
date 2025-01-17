using System.Numerics;
using System.Collections.Generic;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Dynamics;
using SDL2Engine.Core;
using SDL2Engine.Core.Physics.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Physics
{
    public class PhysicsService : IPhysicsService
    {
        // 1 meter = 100 pixels
        private const float PPM = 100f;

        private World m_world;
        private CollisionDetector m_collisionDetector;
        
        private readonly List<GameObject> m_registeredObjects = new List<GameObject>();
        private readonly List<Body> m_boundaryBodies = new List<Body>();
        
        public CollisionDetector CollisionDetector => m_collisionDetector;
        
        // Use positive gravity by default because SDL and Box2d y values are flipped
        public void Initialize(float gravity = 9.81f)
        {
            Vector2 gravityVec = new Vector2(0f, gravity);
            m_world = new World(gravityVec);
            m_collisionDetector = new CollisionDetector(m_world, PPM);
            Debug.Log($"<color=green>PHYSICS ENGINE INITIALIZED gravity: {gravity}</color>");
        }

        /// <summary>
        /// Register a GameObject as a dynamic/kinematic/static body in Box2D.
        /// </summary>
        public void RegisterGameObject(GameObject gameObject, float width, float height, BodyType type)
        {
            BodyDef bodyDef = new BodyDef
            {
                BodyType = type,
                // Convert from pixel coords to meters
                Position = new Vector2(
                    gameObject.Position.X / PPM,
                    gameObject.Position.Y / PPM
                )
            };

            Body body = m_world.CreateBody(bodyDef);

            var diameter = Math.Min(width, height);
            var radius = (diameter / 2f);
            CircleShape shape = new CircleShape();
            shape.Radius = (radius) / PPM; 

            // TODO: Double check this, could be cause for larger box collider than needed
            // PolygonShape shape = new PolygonShape();
            // shape.SetAsBox(
            //     (width  / 2f) / PPM, 
            //     (height / 2f) / PPM
            // );

            FixtureDef fixtureDef = new FixtureDef
            {
                Shape = shape,
                Density = 1.0f,
                Friction = 0.2f
            };
            body.CreateFixture(fixtureDef);

            gameObject.PhysicsBody = body;
            m_registeredObjects.Add(gameObject);
        }

        /// <summary>
        /// Fixed timestep update of Box2D physics. Syncs body positions/rotations back to GameObjects.
        /// </summary>
        public void UpdatePhysics(float deltaTime)
        {
            m_world.Step(deltaTime, velocityIterations: 8, positionIterations: 3);
        }

        /// <summary>
        /// Create static bodies around the window edges.
        /// Assuming top-left is (0,0) in SDL and y grows downward.
        /// So top edge is y=0, bottom edge is y=screenHeight.
        /// </summary>
        public void CreateWindowBoundaries(float screenWidth, float screenHeight)
        {
            foreach (var body in m_boundaryBodies)
            {
                m_world.DestroyBody(body);
            }
            m_boundaryBodies.Clear();

            float w = screenWidth  / PPM;
            float h = screenHeight / PPM;

            m_boundaryBodies.Add(CreateStaticBody(
                new Vector2(w / 2f, 0f),
                w,
                0.1f 
            ));

            m_boundaryBodies.Add(CreateStaticBody(
                new Vector2(w / 2f, h),
                w,
                0.1f
            ));

            m_boundaryBodies.Add(CreateStaticBody(
                new Vector2(0f, h / 2f),
                0.1f,
                h
            ));

            m_boundaryBodies.Add(CreateStaticBody(
                new Vector2(w, h / 2f),
                0.1f,
                h
            ));
        }

        private Body CreateStaticBody(Vector2 position, float width, float height)
        {
            BodyDef bodyDef = new BodyDef
            {
                BodyType = BodyType.StaticBody,
                Position = position
            };
            Body body = m_world.CreateBody(bodyDef);

            // Note: setAsBox expects half-width and half-height (CHECK THIS)
            PolygonShape shape = new PolygonShape();
            shape.SetAsBox(width / 2f, height / 2f);

            FixtureDef fixtureDef = new FixtureDef
            {
                Shape = shape,
                Density = 1.0f,
                Friction = 0.3f,
                Restitution = 0.0f
            };
            body.CreateFixture(fixtureDef);

            return body;
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
