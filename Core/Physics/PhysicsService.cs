using System.Numerics;
using System.Collections.Generic;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Dynamics;
using SDL2Engine.Core;
using SDL2Engine.Core.Physics.Interfaces;
using SDL2Engine.Core.Utils;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuUtilities.Memory;
using SDL2Engine.Core.Physics.Bepu;

namespace SDL2Engine.Core.Physics
{
    public class PhysicsService : IPhysicsService
    {
        /**********************************
         *      BOX 2D INTEGRATION
         **********************************/
        // 1 meter = 100 pixels
        private const float PPM = 100f;

        private World m_world;
        private CollisionDetector m_collisionDetector;
        
        private readonly List<GameObject> m_registeredObjects = new List<GameObject>();
        private readonly List<Body> m_boundaryBodies = new List<Body>();
        
        public CollisionDetector CollisionDetector => m_collisionDetector;
        
        /**********************************
         *      BEPU 3D INTEGRATION
         **********************************/
        private Simulation m_simulation;
        private BufferPool m_bufferPool;
        
        // Use positive gravity by default because SDL and Box2d y values are flipped
        public void Initialize(float gravity = 9.81f)
        {
            if (PlatformInfo.PipelineType == PipelineType.Pipe2D)
            {
                Vector2 gravityVec = new Vector2(0f, gravity);
                m_world = new World(gravityVec);
                m_collisionDetector = new CollisionDetector(m_world, PPM);
                Debug.Log($"<color=green>2D PHYSICS ENGINE INITIALIZED (BOX2D): {gravity}</color>");
            }

            if (PlatformInfo.PipelineType == PipelineType.Pipe3D)
            {
                m_bufferPool = new BufferPool();
                // Use a negative Y gravity vector (Y is up in BEPU by default)
                var poseIntegrator = new DefaultPoseIntegratorCallbacks(new System.Numerics.Vector3(0, -gravity, 0));
                m_simulation = Simulation.Create(
                    m_bufferPool,
                    new DefaultNarrowPhaseCallbacks(),
                    poseIntegrator,
                    new SolveDescription(4, 1));  // 4 substeps, 1 velocity iteration per substep
                Debug.Log($"<color=green>3D PHYSICS ENGINE INITIALIZED (BEPU): {gravity}</color>");
            }
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
            if (PlatformInfo.PipelineType == PipelineType.Pipe2D)
            {
                m_world.Step(deltaTime, velocityIterations: 8, positionIterations: 3);
            }

            if (PlatformInfo.PipelineType == PipelineType.Pipe3D)
            {
                m_simulation.Timestep(deltaTime);
            }
        }

        /// <summary>
        /// Create static bodies around the window edges.
        /// Assuming top-left is (0,0) in SDL and y grows downward.
        /// So top edge is y=0, bottom edge is y=screenHeight.
        /// </summary>
        public void CreateWindowBoundaries(float screenWidth, float screenHeight)
        {
            if (PlatformInfo.PipelineType == PipelineType.Pipe3D)
                return;
            
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

        /// <summary>
        /// Creates a 3D BEPU physics object
        /// </summary>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <param name="mass"></param>
        /// <returns></returns>
        public BodyHandle CreatePhysicsBody(Vector3 position, Vector3 size, float mass)
        {
            // Create the initial pose.
            RigidPose pos = new RigidPose(position);
            Box boxShape = new Box(size.X, size.Y, size.Z);
            BodyInertia inertia = boxShape.ComputeInertia(mass);
            TypedIndex shapeIndex = m_simulation.Shapes.Add(boxShape);
            CollidableDescription collidableDescription = new CollidableDescription(shapeIndex, 0.1f);
            BodyDescription bodyDescription = BodyDescription.CreateDynamic(
                pos, inertia, collidableDescription, new BodyActivityDescription());
    
            // Add the body to the simulation and return its handle.
            BodyHandle bodyHandle = m_simulation.Bodies.Add(in bodyDescription);
            return bodyHandle;
        }
        
        public BodyHandle CreateSpherePhysicsBody(Vector3 position, float radius, float mass)
        {
            // Create the initial pose.
            RigidPose pose = new RigidPose(position);

            // Create a sphere shape using the desired radius.
            Sphere sphereShape = new Sphere(radius);

            // Compute the inertia for a sphere.
            BodyInertia inertia = sphereShape.ComputeInertia(mass);

            // Add the sphere shape to the simulation's shape collection.
            TypedIndex shapeIndex = m_simulation.Shapes.Add(sphereShape);

            // Create a collidable description. The second parameter is the speculative margin.
            CollidableDescription collidableDescription = new CollidableDescription(shapeIndex, 0.1f);

            // Create a dynamic body description for the sphere.
            BodyDescription bodyDescription = BodyDescription.CreateDynamic(
                pose, inertia, collidableDescription, new BodyActivityDescription(0f));

            // Add the body to the simulation and return its handle.
            BodyHandle bodyHandle = m_simulation.Bodies.Add(in bodyDescription);
            return bodyHandle;
        }


        /// <summary>
        /// Creates a Box2D physics object
        /// </summary>
        /// <param name="position"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="type"></param>
        /// <returns></returns>
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

        public void ApplyLinearImpulse(BodyHandle handle, in Vector3 impulse)
        {
            m_simulation.Bodies.GetBodyReference(handle).ApplyLinearImpulse(in impulse);
        }

        /// <summary>
        /// Returns the body reference for the BEPU physics body
        /// </summary>
        /// <param name="bodyHandle"></param>
        public BodyReference GetBodyReference(BodyHandle bodyHandle)
        {
            return m_simulation.Bodies[bodyHandle];
        }

        public void SetContactListener(IContactListener contactListener)
        {
            m_world.SetContactListener(contactListener);
        }
    }
}
