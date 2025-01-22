using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Tasks;
using SDL2Engine.Core.AI.Data;
using SDL2Engine.Core.Input;
using SDL2Engine.Core.Partitions;
using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Engine.Core.AI
{
    public class BoidManager
    {
        private const float NeighborRadius = 32; 
        private const float NeighborRadiusSquared = NeighborRadius * NeighborRadius;
        private const float AlignmentWeight = 2.5f;
        private const float CohesionWeight = 2.0f;
        private const float SeparationWeight = 4.5f;
        private const float DebugMousePullWeight = 1f;
        private const float MinSeparationDistance = 1f; 

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
        /// Update all boids' positions based on flocking behavior, using parallel steering.
        /// </summary>
        public void UpdateBoids(float deltaTime)
        {
            var boidSteerings = new Vector2[_boids.Count];
            Vector2 mousePosition = new Vector2(InputManager.MouseX, InputManager.MouseY);

            // Parallel pass
            Parallel.For(0, _boids.Count, i =>
            {
                var boid = _boids[i];
                var neighbors = _partitioner.GetNeighbors(boid.Position, NeighborRadius);

                Vector2 alignment   = CalculateAlignment(boid, neighbors)   * AlignmentWeight;
                Vector2 cohesion    = CalculateCohesion(boid, neighbors)    * CohesionWeight;
                Vector2 separation  = CalculateSeparation(boid, neighbors)  * SeparationWeight;

                Vector2 toMouse = mousePosition - boid.Position;
                Vector2 mouseAttraction = Vector2.Zero;
                if (toMouse.LengthSquared() > 0)
                    mouseAttraction = Vector2.Normalize(toMouse) * DebugMousePullWeight;

                boidSteerings[i] = alignment + cohesion + separation + mouseAttraction;
            });

            for (int i = 0; i < _boids.Count; i++)
            {
                var boid = _boids[i];

                boid.Velocity += boidSteerings[i] * deltaTime;

                float maxSpeed = _boidSpeed; 
                float speedSq = boid.Velocity.LengthSquared();
                if (speedSq > maxSpeed * maxSpeed)
                {
                    boid.Velocity = Vector2.Normalize(boid.Velocity) * maxSpeed;
                }

                boid.Update(deltaTime);
            }
            _boids.Sort((a, b) => a.Position.Y.CompareTo(b.Position.Y));
        }

        public void RenderBoids(nint renderer, ICameraService cameraService)
        {
            foreach (var boid in _boids)
            {
                boid.Render(renderer, cameraService);
            }
        }

        private Vector2 CalculateAlignment(GameObject boid, IEnumerable<GameObject> neighbors)
        {
            Vector2 averageVelocity = Vector2.Zero;
            int count = 0;
            foreach (var neighbor in neighbors)
            {
                averageVelocity += neighbor.Velocity;
                count++;
            }
            if (count == 0) return Vector2.Zero;
            averageVelocity /= count;
            return averageVelocity - boid.Velocity;
        }

        private Vector2 CalculateCohesion(GameObject boid, IEnumerable<GameObject> neighbors)
        {
            Vector2 centerOfMass = Vector2.Zero;
            int count = 0;
            foreach (var neighbor in neighbors)
            {
                centerOfMass += neighbor.Position;
                count++;
            }
            if (count == 0) return Vector2.Zero;
            centerOfMass /= count;

            Vector2 dir = centerOfMass - boid.Position;
            return (dir.LengthSquared() > 0) ? Vector2.Normalize(dir) : Vector2.Zero;
        }

        private Vector2 CalculateSeparation(GameObject boid, IEnumerable<GameObject> neighbors)
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

            return (separation.LengthSquared() > 0) ? Vector2.Normalize(separation) : Vector2.Zero;
        }
    }
}
