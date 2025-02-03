using OpenTK.Mathematics;
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
                    new Vector3(data.Position.X, data.Position.Y, 0),
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

                var toMouse = mousePosition - new Vector2(boid.Position.X, boid.Position.Y);
                Vector2 mouseAttraction = Vector2.Zero;
                if (toMouse.LengthSquared > 0)
                    mouseAttraction = Vector2.Normalize(toMouse) * DebugMousePullWeight;

                boidSteerings[i] = alignment + cohesion + separation + mouseAttraction;
            });

            for (int i = 0; i < _boids.Count; i++)
            {
                var boid = _boids[i];

                boid.Velocity += new Vector3(boidSteerings[i].X, boidSteerings[i].Y,0) * deltaTime;

                float maxSpeed = _boidSpeed; 
                float speedSq = boid.Velocity.LengthSquared;
                if (speedSq > maxSpeed * maxSpeed)
                {
                    boid.Velocity = Vector3.Normalize(boid.Velocity) * maxSpeed;
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
                averageVelocity += new Vector2(neighbor.Velocity.X, neighbor.Velocity.Y);
                count++;
            }
            if (count == 0) return Vector2.Zero;
            averageVelocity /= count;
            return averageVelocity - new Vector2(boid.Velocity.X, boid.Velocity.Y);
        }

        private Vector2 CalculateCohesion(GameObject boid, IEnumerable<GameObject> neighbors)
        {
            Vector3 centerOfMass = Vector3.Zero;
            int count = 0;
            foreach (var neighbor in neighbors)
            {
                centerOfMass += neighbor.Position;
                count++;
            }
            if (count == 0) return Vector2.Zero;
            centerOfMass /= count;

            Vector3 dir = centerOfMass - boid.Position;
            var result = (dir.LengthSquared > 0) ? Vector3.Normalize(dir) : Vector3.Zero;
            return new Vector2(result.X,result.Y);
        }

        private Vector2 CalculateSeparation(GameObject boid, IEnumerable<GameObject> neighbors)
        {
            Vector2 separation = Vector2.Zero;

            foreach (var neighbor in neighbors)
            {
                var diff = boid.Position - neighbor.Position;
                float distanceSquared = diff.LengthSquared;
                if (distanceSquared > 0 && distanceSquared < NeighborRadiusSquared)
                {
                    float distance = MathF.Sqrt(distanceSquared);
                    if (distance < MinSeparationDistance)
                    {
                        float strength = (MinSeparationDistance - distance) / MinSeparationDistance;
                        var pos = Vector3.Normalize(diff) * strength * 10.0f;
                        separation += new Vector2(pos.X, pos.Y);
                    }
                    else
                    {
                        float strength = 1.0f / distanceSquared;
                        var result = diff * strength;
                        separation += new Vector2(result.X,result.Y);
                    }
                }
            }

            return (separation.LengthSquared > 0) ? Vector2.Normalize(separation) : Vector2.Zero;
        }
    }
}
