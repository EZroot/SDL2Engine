using System.Collections.Concurrent;
using System.Numerics;
using SDL2Engine.Core.AI.Data;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Partitions;
using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Engine.Core.AI
{
    public class BoidManager
    {
        private const float NeighborRadius = 64; 
        private const float NeighborRadiusSquared = NeighborRadius * NeighborRadius;
        private const float AlignmentWeight = 1.5f ;
        private const float CohesionWeight = 1.0f ;
        private const float SeparationWeight = 10.5f;
        private const float DebugMousePullWeight = 2.5f;
        private const float MinSeparationDistance = 64.0f; 

        private readonly List<GameObject> _boids = new();
        private readonly SpatialPartitioner _partitioner;
        private readonly float _worldSize;
        private readonly float _boidSpeed;

        public BoidManager(SpatialPartitioner partitioner, float worldSize, float boidSpeed)
        {
            _partitioner = partitioner;
            _worldSize = worldSize;
            _boidSpeed = boidSpeed;
        }

        public void InitializeBoids(BoidGroupData[] boidGroupData)
        {
            foreach (var data in boidGroupData)
            {
                var gameObject = new GameObject(
                    data.Sprite,
                    data.Position,
                    data.Scale,
                    _partitioner);
                _boids.Add(gameObject);
            }
        }

        /// <summary>
        /// Update all boids' positions based on flocking behavior
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateBoids(float deltaTime)
        {
            List<GameObject> neighborBuffer = new List<GameObject>(128);

            Vector2 mousePosition = new Vector2(InputManager.MouseX, InputManager.MouseY);

            foreach (var boid in _boids)
            {
                neighborBuffer.Clear();
                neighborBuffer.AddRange(_partitioner.GetNeighbors(boid.Position, NeighborRadius));

                // Calculate steering behaviors
                Vector2 alignment = CalculateAlignment(boid, neighborBuffer) * AlignmentWeight;
                Vector2 cohesion = CalculateCohesion(boid, neighborBuffer) * CohesionWeight;
                Vector2 separation = CalculateSeparation(boid, neighborBuffer) * SeparationWeight;

                Vector2 mouseAttraction = Vector2.Zero;
                Vector2 toMouse = mousePosition - boid.Position;
                if (toMouse.LengthSquared() > 0)
                {
                    mouseAttraction = Vector2.Normalize(toMouse) * DebugMousePullWeight;
                }

                Vector2 steering = alignment + cohesion + separation + mouseAttraction;
                boid.Velocity += steering * deltaTime;

                if (boid.Velocity.LengthSquared() > _boidSpeed * _boidSpeed)
                {
                    boid.Velocity = Vector2.Normalize(boid.Velocity) * _boidSpeed;
                }

                boid.Update(deltaTime);
            }
        }

        public void RenderBoids(nint renderer, ICameraService cameraService)
        {
            foreach (var boid in _boids)
            {
                boid.Render(renderer, cameraService);
            }
        }

        private Vector2 CalculateAlignment(GameObject boid, List<GameObject> neighbors)
        {
            if (neighbors.Count == 0) return Vector2.Zero;

            Vector2 averageVelocity = Vector2.Zero;
            foreach (var neighbor in neighbors)
            {
                averageVelocity += neighbor.Velocity;
            }

            averageVelocity /= neighbors.Count;

            // Steering towards the average velocity
            Vector2 alignment = averageVelocity - boid.Velocity;
            return alignment;
        }

        private Vector2 CalculateCohesion(GameObject boid, List<GameObject> neighbors)
        {
            if (neighbors.Count == 0) return Vector2.Zero;

            Vector2 centerOfMass = Vector2.Zero;
            foreach (var neighbor in neighbors)
            {
                centerOfMass += neighbor.Position;
            }

            centerOfMass /= neighbors.Count;
            Vector2 direction = centerOfMass - boid.Position;
            return direction.LengthSquared() > 0 ? Vector2.Normalize(direction) : Vector2.Zero;
        }

        private Vector2 CalculateSeparation(GameObject boid, List<GameObject> neighbors)
        {
            Vector2 separation = Vector2.Zero;

            foreach (var neighbor in neighbors)
            {
                Vector2 diff = boid.Position - neighbor.Position;
                float distanceSquared = diff.LengthSquared();

                if (distanceSquared > 0 && distanceSquared < NeighborRadiusSquared)
                {
                    float distance = MathF.Sqrt(distanceSquared);
                    if (distance < MinSeparationDistance)
                    {
                        float strength = (MinSeparationDistance - distance) / MinSeparationDistance;
                        separation += Vector2.Normalize(diff) * strength * 10.0f; 
                    }
                    else
                    {
                        float strength = 1.0f / distanceSquared;
                        separation += diff * strength;
                    }
                }
            }

            return separation.LengthSquared() > 0 ? Vector2.Normalize(separation) : Vector2.Zero;
        }
    }
}
